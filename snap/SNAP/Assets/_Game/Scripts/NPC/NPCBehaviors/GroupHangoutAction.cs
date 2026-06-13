using System.Collections;
using UnityEngine;
using GPOyun.Core;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.UtilityAI
{
    public class GroupHangoutAction : MoveAction
    {
        private SocialGroup _hangoutGroup;
        private Vector3 _destination;
        private bool _isLeader;

        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "GroupHangout";
        }

        public override float CalculateUtility()
        {
            // Only trigger if we have very high social desire and decent energy
            if (Needs.SocialDesire < 60f || Needs.Energy < 30f) return 0f;

            // Wait, this is a complex action. Are there 2 other friends nearby?
            Collider[] hits = Physics.OverlapSphere(transform.position, 25f);
            int friendsCount = 0;

            foreach (var hit in hits)
            {
                var otherNpc = hit.GetComponentInParent<NPCController>();
                if (otherNpc != null && otherNpc != Controller)
                {
                    int relation = RelationshipMatrix.Instance != null ? 
                        RelationshipMatrix.Instance.GetRelationship(Controller.NpcId, otherNpc.NpcId) : 0;
                    
                    if (relation >= 40) friendsCount++;
                }
            }

            // Only highly motivated to initiate a group hangout if we have friends around
            if (friendsCount >= 2) return BaseUtility + Needs.SocialDesire * 1.2f;

            return 0f;
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            // 1. Gather the friends
            if (SocialGroupManager.Instance != null)
            {
                _hangoutGroup = SocialGroupManager.Instance.CreateGroup(Controller, transform.position);
                _isLeader = true;
                
                // Invite friends
                Collider[] hits = Physics.OverlapSphere(transform.position, 25f);
                foreach (var hit in hits)
                {
                    if (_hangoutGroup.IsFull) break;

                    var otherNpc = hit.GetComponentInParent<NPCController>();
                    if (otherNpc != null && otherNpc != Controller)
                    {
                        int relation = RelationshipMatrix.Instance != null ? 
                            RelationshipMatrix.Instance.GetRelationship(Controller.NpcId, otherNpc.NpcId) : 0;
                        
                        if (relation >= 40)
                        {
                            _hangoutGroup.AddMember(otherNpc);
                            // Interrupt their current action to join the hangout
                            var planner = otherNpc.GetComponent<NPCActionPlanner>();
                            if (planner != null)
                            {
                                planner.ForceAction(this.GetType()); // Tell them to execute GroupHangoutAction
                            }
                        }
                    }
                }
            }

            // If we are executing this but NOT the leader, we need to find our group
            if (!_isLeader && SocialGroupManager.Instance != null)
            {
                // Wait for the leader to tell us where to go, or find the group we belong to
                // For simplicity, we just join the nearest active group led by a friend
                _hangoutGroup = SocialGroupManager.Instance.TryJoinNearbyGroup(Controller, 40f);
            }

            if (_hangoutGroup == null)
            {
                _isExecuting = false;
                yield break;
            }

            // 2. The Leader picks a faraway spot
            if (_isLeader)
            {
                Controller.TriggerReaction("🗺️", Color.yellow);
                
                // Pick a spot 40-60 meters away
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(40f, 60f);
                _destination = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                _destination.y = transform.position.y;
                _hangoutGroup.UpdateCenter(_destination);

                // Wait for friends to gather before leaving
                yield return new WaitForSeconds(3f);
                Controller.TriggerReaction("🚶", Color.white);
            }
            else
            {
                // Followers wait a bit for the leader to decide
                yield return new WaitForSeconds(2f);
                _destination = _hangoutGroup.CenterPosition;
                Controller.TriggerReaction("👍", Color.green);
            }

            // 3. Walk to the destination
            while (_isExecuting && Vector3.Distance(transform.position, _destination) > 3f)
            {
                // Synchronize speed - if follower is far behind, speed up
                if (!_isLeader)
                {
                    float distToLeader = Vector3.Distance(transform.position, _hangoutGroup.Members[0].transform.position);
                    if (distToLeader > 10f)
                    {
                        yield return MoveToTarget(_destination, NPCState.Traveling, 1.0f, 6f); // Run to catch up
                    }
                    else
                    {
                        yield return MoveToTarget(_destination, NPCState.Traveling, 1.0f, 3.5f); // Walk with them
                    }
                }
                else
                {
                    yield return MoveToTarget(_destination, NPCState.Traveling, 1.0f, 3.5f); // Leader walks normal pace
                }

                if (!_isExecuting) break;
            }

            // 4. Stand in circle and hangout
            if (_isExecuting)
            {
                Controller.TriggerReaction("🎉", Color.cyan);
                
                // Add a small random offset so they stand in a circle
                float angle = Random.Range(0, Mathf.PI * 2);
                Vector3 standPos = _destination + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 1.5f;

                yield return MoveToTarget(standPos, NPCState.Socializing, 0.5f, 5f);

                // Face the center
                Vector3 lookDir = (_destination - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);

                // Hang out for a long time
                Needs.SatisfySocial(100f);
                Needs.SatisfyBoredom(100f);
                yield return new WaitForSeconds(Random.Range(10f, 20f));
            }

            // Cleanup
            if (SocialGroupManager.Instance != null && _hangoutGroup != null)
            {
                SocialGroupManager.Instance.LeaveGroup(Controller, _hangoutGroup);
            }

            _isExecuting = false;
        }
    }
}
