using UnityEngine;

namespace GPOyun.CameraSystem
{
    /// <summary>
    /// Decoupled manager that handles camera aiming and subject alignment checks.
    /// </summary>
    public class ViewfinderManager : MonoBehaviour
    {
        public static ViewfinderManager Instance { get; private set; }

        [Header("Aim Settings")]
        [SerializeField] private float maxDistance = 45f;
        [SerializeField] private LayerMask checkLayers = ~0;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsAimingAtSubject(Camera cam, out Environment.PhotoSubject subject)
        {
            subject = null;
            if (cam == null) return false;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, checkLayers))
            {
                subject = hit.collider.GetComponentInParent<Environment.PhotoSubject>() 
                          ?? hit.collider.GetComponent<Environment.PhotoSubject>();
                
                return subject != null;
            }

            return false;
        }

        public int CalculateCompositionScore(Camera cam, Environment.PhotoSubject subject)
        {
            if (cam == null || subject == null) return 0;

            // Base score based on distance (closer/perfect distance matches better)
            float dist = Vector3.Distance(cam.transform.position, subject.transform.position);
            int distScore = 100 - Mathf.RoundToInt(Mathf.Clamp(Mathf.Abs(dist - 8f) * 4f, 0f, 60f));

            // Viewport alignment (closer to center means better composition)
            Vector3 viewportPos = cam.WorldToViewportPoint(subject.transform.position);
            float offsetFromCenter = Vector2.Distance(new Vector2(viewportPos.x, viewportPos.y), new Vector2(0.5f, 0.5f));
            int alignmentScore = 100 - Mathf.RoundToInt(Mathf.Clamp(offsetFromCenter * 200f, 0f, 40f));

            int total = Mathf.Clamp((distScore + alignmentScore) / 2, 10, 100);
            return total;
        }
    }
}
