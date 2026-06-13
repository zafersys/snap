using System.Collections;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class GoHomeAction : MoveAction
    {
        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "GoHome";
        }

        public override float CalculateUtility()
        {
            float utility = BaseUtility;
            if (Needs.IsNightTime) utility += 80f; // Strongly want to go home
            if (Needs.Energy < 15f) utility += 50f; // So tired they just want to go home
            return utility;
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            yield return MoveToTarget(Controller.homePosition, NPCState.WalkingHome, 1.2f, 8f);
            if (!_isExecuting) yield break;

            Controller.EnterState(NPCState.Idle);
            
            while (_isExecuting && Needs.IsNightTime)
            {
                Needs.RestoreEnergy(3.0f * Time.deltaTime);
                yield return null;
            }

            _isExecuting = false;
        }
    }
}
