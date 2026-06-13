using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using GPOyun.Core;

namespace GPOyun.NPC.UtilityAI
{
    public class TravelAction : MoveAction
    {
                
        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "TravelOutsideVillage";
            _agent = controller.GetComponent<NavMeshAgent>();
        }

        public override float CalculateUtility()
        {
            // Low chance/utility by default, increases if high energy and low boredom?
            // Just a flat chance or based on a new need if added. For now base utility is 5
            // But occasionally spikes if they haven't traveled in a while.
            // We'll give it a low static utility, but random spikes.
            float utility = BaseUtility + 5f; // Random removed to prevent oscillation
            
            if (Needs.Energy < 40f)
            {
                utility -= 50f; // Won't travel if tired
            }

            return Mathf.Max(0, utility);
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;
            Controller.EnterState(NPCState.Traveling);

            // Pick a scenic location way outside the village boundaries (radial distance of 24-34m)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float travelDistance = Random.Range(24f, 34f);
            Vector3 outsideTarget = new Vector3(Mathf.Cos(angle) * travelDistance, transform.position.y, Mathf.Sin(angle) * travelDistance);

            
            yield return MoveToTarget(outsideTarget, NPCState.Traveling, 1.2f, 8f);
            if (!_isExecuting) yield break;
            
            Controller.TriggerReaction("[TRAVEL]", new Color(0.4f, 0.8f, 0.4f));
            if (JournalManager.Instance != null && Random.value < 0.4f)
            {
                JournalManager.Instance.AddObservation($"[{Controller.NpcName}] [TRAVEL] -> [OUTSIDE-VILLAGE] \\o/", new Color(0.4f, 0.8f, 0.4f));
            }

                // Arrived! Stay at the viewpoint and admire the countryside landscape
                Controller.EnterState(NPCState.Idle);
                yield return new WaitForSeconds(Random.Range(10f, 18f));
                
                // Need fulfilled
                Needs.SatisfyBoredom(40f);

            _isExecuting = false;
        }

        
    }
}
