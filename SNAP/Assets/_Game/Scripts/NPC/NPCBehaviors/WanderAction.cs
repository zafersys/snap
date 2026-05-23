using System.Collections;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class WanderAction : MoveAction
    {
        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "Wander";
        }

        public override float CalculateUtility()
        {
            float utility = BaseUtility + (Needs.Boredom * 0.8f);
            if (Needs.Energy < 20f) utility -= 30f;
            return Mathf.Max(0, utility);
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;
            Controller.EnterState(NPCState.Wandering);

            while (_isExecuting)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += transform.position;
                
                yield return MoveToTarget(randomDirection, NPCState.Wandering, 1.2f, 5f);
                if (!_isExecuting) yield break;
                
                Needs.SatisfyBoredom(15f);

                Controller.EnterState(NPCState.Idle);
                float waitTime = Random.Range(2f, 5f);
                for(float t=0; t<waitTime; t+=Time.deltaTime)
                {
                    if (!_isExecuting) yield break;
                    yield return null;
                }
                Controller.EnterState(NPCState.Wandering);
            }
        }
    }
}
