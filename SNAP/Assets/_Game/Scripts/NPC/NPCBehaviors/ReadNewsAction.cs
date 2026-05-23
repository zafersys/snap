using System.Collections;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class ReadNewsAction : MoveAction
    {
        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "ReadNews";
        }

        public override float CalculateUtility()
        {
            if (Needs.HasPendingNews && Controller.boardPosition != null)
            {
                return 100f; 
            }
            return 0f;
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            if (Controller.boardPosition != null)
            {
                yield return MoveToTarget(Controller.boardPosition.position, NPCState.WalkingToBoard, 1.2f, 8f);
                if (!_isExecuting) yield break;

                Controller.EnterState(NPCState.Reading);
                Needs.HasPendingNews = false;

                yield return new WaitForSeconds(Random.Range(3f, 7f));
                
                Controller.EnterState(NPCState.Idle);
            }

            _isExecuting = false;
        }
    }
}
