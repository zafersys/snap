using System.Collections;
using UnityEngine;
using GPOyun.Core;

namespace GPOyun.NPC.UtilityAI
{
    public class FleePlayerAction : MoveAction
    {
        private Player.PlayerController _player;

        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "FleePlayer";
        }

        public override float CalculateUtility()
        {
            if (Needs.IsPlayerHostile && Needs.IsPlayerNearby) return 120f; 
            return 0f;
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            if (_player == null)
            {
                _player = FindAnyObjectByType<Player.PlayerController>();
            }

            if (_player != null)
            {
                Vector3 dirAway = (transform.position - _player.transform.position).normalized;
                Vector3 targetPosition = transform.position + dirAway * 8f;
                
                Controller.TriggerReaction("[ANGRY]", VisualUtils.Terracotta);
                var gestures = Controller.GetComponent<PantomimeGestures>();
                if (gestures != null) gestures.PlayFear();

                Controller.moveSpeed = Controller.runSpeed;
                yield return MoveToTarget(targetPosition, NPCState.Fleeing, 1.2f, 3f);
                Controller.moveSpeed = 1.8f;

                if (!_isExecuting) yield break;

                Controller.EnterState(NPCState.Idle);
                Needs.IsPlayerNearby = false; 
            }

            _isExecuting = false;
        }
    }
}
