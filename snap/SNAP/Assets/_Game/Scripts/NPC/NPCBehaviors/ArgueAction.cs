using System.Collections;
using UnityEngine;
using GPOyun.Core;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.UtilityAI
{
    public class ArgueAction : MoveAction
    {
        private NPCController _targetNPC;

        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "Argue";
        }

        public override float CalculateUtility()
        {
            // Only consider arguing if we are bored or have high social desire but in a bad way
            if (Needs.Boredom < 40f && Needs.SocialDesire < 50f) return 0f;

            _targetNPC = FindEnemy();
            if (_targetNPC == null) return 0f;

            int relation = RelationshipMatrix.Instance != null ? 
                RelationshipMatrix.Instance.GetRelationship(Controller.NpcId, _targetNPC.NpcId) : 0;

            // The worse the relationship, the higher the utility to go pick a fight!
            float utility = BaseUtility + (-relation * 0.8f) + (Needs.Boredom * 0.5f);
            return Mathf.Max(0, utility);
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            while (_isExecuting)
            {
                if (_targetNPC == null) break;

                // Stomp over to the enemy
                yield return MoveToTarget(_targetNPC.transform.position, NPCState.Wandering, 2.0f, 5f);
                if (!_isExecuting || _targetNPC == null) break;

                // Face them
                Vector3 lookDir = (_targetNPC.transform.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);

                // Start the argument
                Controller.currentEmotion = EmotionType.Angry;
                Controller.TriggerReaction("🤬", new Color(0.9f, 0.2f, 0.2f));

                var gestures = Controller.GetComponent<PantomimeGestures>();
                if (gestures != null) gestures.SetSadness(true); // Temporary visual for negative interaction

                if (RelationshipMatrix.Instance != null)
                {
                    // A's relationship drops a bit (they let off steam)
                    RelationshipMatrix.Instance.ModifyRelationship(Controller.NpcId, _targetNPC.NpcId, -10);
                    // B's relationship drops MASSIVELY because A just walked up and yelled at them
                    RelationshipMatrix.Instance.ModifyRelationship(_targetNPC.NpcId, Controller.NpcId, -30);
                    
                    // Form a toxic opinion
                    RelationshipMatrix.Instance.RecordOpinion(Controller.NpcId, _targetNPC.NpcId, "They made me so angry today! I can't stand them.");
                    RelationshipMatrix.Instance.RecordOpinion(_targetNPC.NpcId, Controller.NpcId, "They came over just to pick a fight with me. Jerk.");
                }

                // They both remember the fight
                var memStream = Controller.GetComponent<Memory.NPCMemoryStream>();
                if (memStream != null)
                {
                    memStream.AddMemory(new MemoryEvent(Time.time, StimulusType.HostileConfrontation, _targetNPC.NpcId, EmotionType.Angry));
                }

                var targetMem = _targetNPC.GetComponent<Memory.NPCMemoryStream>();
                if (targetMem != null)
                {
                    targetMem.AddMemory(new MemoryEvent(Time.time, StimulusType.HostileConfrontation, Controller.NpcId, EmotionType.Angry));
                    _targetNPC.TriggerReaction("💢", new Color(1f, 0.3f, 0.3f));
                }

                Needs.SatisfySocial(50f); // Arguing is socializing, technically
                Needs.SatisfyBoredom(80f); // It's definitely not boring

                yield return new WaitForSeconds(Random.Range(3f, 6f));
                break;
            }

            _isExecuting = false;
        }

        private NPCController FindEnemy()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 25f);
            NPCController worstEnemy = null;
            int lowestRelation = 0; // Only look for people we have negative relations with

            foreach (var hit in hits)
            {
                var otherNpc = hit.GetComponentInParent<NPCController>();
                if (otherNpc != null && otherNpc != Controller)
                {
                    int relation = RelationshipMatrix.Instance != null ? 
                        RelationshipMatrix.Instance.GetRelationship(Controller.NpcId, otherNpc.NpcId) : 0;
                    
                    if (relation < lowestRelation)
                    {
                        lowestRelation = relation;
                        worstEnemy = otherNpc;
                    }
                }
            }
            return worstEnemy;
        }
    }
}
