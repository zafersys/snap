using System.Collections;
using UnityEngine;
using GPOyun.Core;

namespace GPOyun.NPC.UtilityAI
{
    public class GossipAction : MoveAction
    {
        private NPCController _targetNPC;

        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "Gossip";
        }

        public override float CalculateUtility()
        {
            var memoryStream = Controller.GetComponent<Memory.NPCMemoryStream>();
            if (memoryStream != null && memoryStream.GetMemorySnapshot().Count > 0)
            {
                return BaseUtility + (Needs.SocialDesire * 0.7f);
            }
            return 0f;
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            while (_isExecuting)
            {
                _targetNPC = FindFriendNPC();

                if (_targetNPC == null)
                {
                    yield return new WaitForSeconds(2f);
                    break;
                }

                yield return MoveToTarget(_targetNPC.transform.position, NPCState.Socializing, 2.0f, 5f);
                if (!_isExecuting || _targetNPC == null) break;

                Vector3 lookDir = (_targetNPC.transform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);

                Controller.TriggerReaction("🗣️", new Color(0.9f, 0.4f, 0.8f));
                
                var gestures = Controller.GetComponent<PantomimeGestures>();
                if (gestures != null) gestures.PlayJoy(); 

                var myMemories = Controller.GetComponent<Memory.NPCMemoryStream>().GetMemorySnapshot();
                if (myMemories.Count > 0)
                {
                    var memoryToShare = myMemories[Random.Range(0, myMemories.Count)];
                    var targetMemStream = _targetNPC.GetComponent<Memory.NPCMemoryStream>();
                    if (targetMemStream != null)
                    {
                        targetMemStream.AddMemory(memoryToShare);
                        if (memoryToShare.FeltEmotion == EmotionType.Angry || memoryToShare.FeltEmotion == EmotionType.Fearful)
                        {
                            _targetNPC.relationshipWithPlayer = Mathf.Clamp(_targetNPC.relationshipWithPlayer - 10, -100, 100);
                        }
                    }
                }

                Needs.SatisfySocial(80f);
                yield return new WaitForSeconds(3f);
                break;
            }

            _isExecuting = false;
        }

        private NPCController FindFriendNPC()
        {
            if (RelationshipMatrix.Instance == null) return null;

            Collider[] hits = Physics.OverlapSphere(transform.position, 20f);
            foreach (var hit in hits)
            {
                var otherNpc = hit.GetComponentInParent<NPCController>();
                if (otherNpc != null && otherNpc != Controller)
                {
                    if (RelationshipMatrix.Instance.GetRelationship(Controller.NpcId, otherNpc.NpcId) > 20)
                    {
                        return otherNpc;
                    }
                }
            }
            return null;
        }
    }
}
