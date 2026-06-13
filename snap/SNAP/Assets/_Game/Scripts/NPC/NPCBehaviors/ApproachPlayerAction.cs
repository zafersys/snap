using System.Collections;
using UnityEngine;
using GPOyun.Core;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.UtilityAI
{
    public class ApproachPlayerAction : MoveAction
    {
        private Transform _playerTransform;
        private bool _hasMetPlayer = false;

        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            ActionName = "ApproachPlayer";
        }

        public override float CalculateUtility()
        {
            // If they've already introduced themselves recently, don't do it again
            if (_hasMetPlayer) return 0f;

            // They must be somewhat bored and have high social desire
            if (Needs.SocialDesire < 30f) return 0f;

            // Find player
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _playerTransform = player.transform;
            }

            if (_playerTransform == null) return 0f;

            // If player is close enough to see
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist > 30f) return 0f; // Too far

            // If they see the player, and they haven't met them, HIGH utility to go say hi!
            float utility = BaseUtility + 60f; 
            
            // If they actually LIKE the player, they are even more likely to come say hi
            if (Controller.relationshipWithPlayer > 20) utility += 30f;

            return Mathf.Max(0, utility);
        }

        public override IEnumerator Execute()
        {
            _isExecuting = true;

            while (_isExecuting)
            {
                if (_playerTransform == null) break;

                // Move to player
                yield return MoveToTarget(_playerTransform.position, NPCState.Wandering, 3.0f, 6f);
                if (!_isExecuting || _playerTransform == null) break;

                // Face player
                Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);

                // Introduce!
                Controller.currentEmotion = EmotionType.Happy;
                
                if (Controller.relationshipWithPlayer > 20)
                {
                    Controller.TriggerReaction("🥰", new Color(1f, 0.4f, 0.6f)); // Happy to see you!
                }
                else
                {
                    Controller.TriggerReaction("👋", new Color(0.9f, 0.9f, 0.9f)); // Polite wave
                    Controller.relationshipWithPlayer += 10; // Boost relation for saying hi
                }

                var gestures = Controller.GetComponent<PantomimeGestures>();
                if (gestures != null) gestures.PlayAffection();

                Needs.SatisfySocial(40f);
                
                _hasMetPlayer = true; // Don't spam the player

                // Stand there and smile for a few seconds
                yield return new WaitForSeconds(Random.Range(3f, 5f));
                
                // Allow meeting again after a long cooldown (e.g. 5 minutes)
                StartCoroutine(ResetMeetCooldown());
                break;
            }

            _isExecuting = false;
        }

        private IEnumerator ResetMeetCooldown()
        {
            yield return new WaitForSeconds(300f);
            _hasMetPlayer = false;
        }
    }
}
