using System.Collections;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class ChillAloneAction : MoveAction
    {
        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "ChillAlone";
        }

        public override float CalculateUtility()
        {
            float utility = BaseUtility + (Needs.Introversion * 0.8f);
            if (Needs.Energy < 30f) utility += 20f; 
            return Mathf.Max(0, utility);
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist  = Random.Range(12f, 18f);
            Vector3 scenicSpot = new Vector3(
                Mathf.Cos(angle) * dist,
                transform.position.y,
                Mathf.Sin(angle) * dist
            );

            yield return MoveToTarget(scenicSpot, NPCState.Wandering, 0.8f, 8f);
            if (!_isExecuting) yield break;

            Controller.EnterState(NPCState.Sitting);
            Controller.TriggerReaction("[CHILL]", new Color(0.6f, 0.9f, 0.9f));

            if (GPOyun.Core.JournalManager.Instance != null && Random.value < 0.35f)
            {
                GPOyun.Core.JournalManager.Instance.AddObservation($"[{Controller.NpcName}] [CHILL] [ALONE] ~_~", new Color(0.6f, 0.9f, 0.9f));
            }

            float chillTime = Random.Range(15f, 28f);
            for (float t = 0; t < chillTime; t += Time.deltaTime)
            {
                if (!_isExecuting) yield break;
                Needs.RestoreEnergy(1.0f * Time.deltaTime);
                Needs.SatisfyIntroversion(4.0f * Time.deltaTime);
                yield return null;
            }

            Controller.EnterState(NPCState.Idle);
            _isExecuting = false;
        }
    }
}
