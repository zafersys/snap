using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GPOyun.UI
{
    /// <summary>
    /// A1 Level Splash Controller
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup splashCanvasGroup;

        public void Initialize(CanvasGroup cg, Text title)
        {
            splashCanvasGroup = cg;
        }

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float holdDuration = 1.0f;

        private void Start()
        {
            if (splashCanvasGroup != null)
            {
                splashCanvasGroup.alpha = 1f;
                splashCanvasGroup.blocksRaycasts = true;
            }

            // Auto-fade after hold duration
            Invoke(nameof(StartFadeOut), holdDuration);
        }

        public void StartFadeOut()
        {
            StartCoroutine(FadeSequence());
        }

        private IEnumerator FadeSequence()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (splashCanvasGroup != null)
                {
                    splashCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
                }
                yield return null;
            }

            if (splashCanvasGroup != null)
            {
                splashCanvasGroup.alpha = 0f;
                splashCanvasGroup.blocksRaycasts = false;
            }
            
            Debug.Log("[Splash] Fade complete. World visible.");
            Destroy(gameObject, 0.5f);
        }
    }
}
