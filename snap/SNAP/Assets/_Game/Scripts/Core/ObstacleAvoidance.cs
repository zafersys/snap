using UnityEngine;

namespace GPOyun.Core
{
    /// <summary>
    /// Surgical proximity sensor that detects Obstacles (Layer 7) 
    /// and calculates an avoidance vector to prevent collisions.
    /// </summary>
    public class ObstacleAvoidance : MonoBehaviour
    {
        [Header("Settings")]
        public float DetectionRange = 3f;
        public float AvoidanceStrength = 5f;
        private int _obstacleMask;

        private void Awake()
        {
            _obstacleMask = (1 << 7); // Obstacle Layer
        }

        public Vector3 GetAvoidanceVector(Vector3 currentDir)
        {
            if (currentDir.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 avoidance = Vector3.zero;
            
            // 1. Center Ray
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, currentDir, out RaycastHit hit, DetectionRange, _obstacleMask))
            {
                // Push away from the surface normal
                avoidance += hit.normal * AvoidanceStrength;
            }

            // 2. Left Diagonal Scan
            Vector3 leftDir = Quaternion.Euler(0, -30, 0) * currentDir;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, leftDir, out RaycastHit leftHit, DetectionRange * 0.7f, _obstacleMask))
            {
                avoidance += leftHit.normal * (AvoidanceStrength * 0.5f);
            }

            // 3. Right Diagonal Scan
            Vector3 rightDir = Quaternion.Euler(0, 30, 0) * currentDir;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, rightDir, out RaycastHit rightHit, DetectionRange * 0.7f, _obstacleMask))
            {
                avoidance += rightHit.normal * (AvoidanceStrength * 0.5f);
            }

            return avoidance;
        }
    }
}
