using UnityEngine;

namespace GPOyun.CameraSystem
{
    /// <summary>
    /// Smoothly follows a target (usually the player) with a fixed offset.
    /// Derived from the Mediterranean 'LoL-style' top-down perspective.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Focus")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float smoothSpeed = 0.125f;

        private void Start()
        {
            // Auto-find player if not assigned
            if (target == null)
            {
                var player = GameObject.Find("Player_Mock") ?? GameObject.FindWithTag("Player");
                if (player != null) target = player.transform;
            }

            // If offset is zero, initialize it with current relative position
            if (offset == Vector3.zero && target != null)
            {
                offset = transform.position - target.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null) 
            {
                // Last ditch effort to find player if they were destroyed/respawned
                var player = GameObject.Find("Player_Mock") ?? GameObject.FindWithTag("Player");
                if (player != null) target = player.transform;
                else return;
            }

            Vector3 desiredPosition = target.position + offset;
            
            // Frame-rate independent smoothing
            // We use a higher multiplier (25f) for ultra-responsive 'LoL-style' tracking
            float t = 1f - Mathf.Exp(-smoothSpeed * 25f * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            offset = transform.position - target.position;
        }
    }
}
