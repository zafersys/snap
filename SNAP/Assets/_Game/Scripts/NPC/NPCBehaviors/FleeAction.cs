using System.Collections;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class FleeAction : MoveAction
    {
        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "Flee";
        }

        public override float CalculateUtility()
        {
            if (Controller.currentEmotion == EmotionType.Fearful) return 100f; 
            return 0f;
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;
            
            var gestures = Controller.GetComponent<PantomimeGestures>();
            if (gestures != null) gestures.PlayFear();

            while (_isExecuting && Controller.currentEmotion == EmotionType.Fearful)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
                    Vector3 fleeTarget = transform.position + fleeDirection * 15f;

                    Controller.moveSpeed = Controller.runSpeed; // Temporarily boost speed
                    yield return MoveToTarget(fleeTarget, NPCState.Fleeing, 1.2f, 3f);
                    Controller.moveSpeed = 1.8f; // Reset
                }

                if (!_isExecuting) yield break;

                Controller.EnterState(NPCState.Idle);
                yield return new WaitForSeconds(3f);

                Controller.currentEmotion = EmotionType.Neutral;
                break;
            }

            _isExecuting = false;
        }
    }
}
