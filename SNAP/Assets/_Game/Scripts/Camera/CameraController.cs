using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using GPOyun.Newspaper;
using GPOyun.UI;
using GPOyun.Core;

namespace GPOyun.CameraSystem
{
    /// <summary>
    /// A1 Camera Controller — Photo capture with RenderTexture snapshot.
    /// C (hold) = raise viewfinder. Space or LMB = capture. 
    /// Uses ONLY the New Input System — no legacy Input.GetAxis.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Settings")]
        public float captureCooldown = 1.5f;
        public float pitchSpeed      = 80f;
        public float pitchMin        = -30f;
        public float pitchMax        = 60f;

        private float _cooldownTimer;
        private bool  _isViewfinderActive;
        private float _currentPitch;

        // Flash effect
        private float _flashTimer;
        private const float FLASH_DURATION = 0.15f;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                return;
            }

            HandleLook();
            HandleViewfinder();
            if (_cooldownTimer > 0) _cooldownTimer -= Time.deltaTime;
            if (_flashTimer   > 0) _flashTimer   -= Time.deltaTime;
        }

        // ─── Vertical Look (Mouse Y) ───────────────────────────────────────
        private void HandleLook()
        {
            var mouse = Mouse.current;
            if (mouse == null || Cursor.lockState != CursorLockMode.Locked) return;

            float mouseY = mouse.delta.ReadValue().y;
            _currentPitch = Mathf.Clamp(_currentPitch - mouseY * pitchSpeed * Time.deltaTime, pitchMin, pitchMax);
            transform.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
        }

        // ─── Viewfinder Toggle & Capture ──────────────────────────────────
        private void HandleViewfinder()
        {
            var keyboard = Keyboard.current;
            var mouse    = Mouse.current;
            if (keyboard == null) return;

            if (PhotoReviewUI.Instance != null && PhotoReviewUI.Instance.IsOpen)
            {
                _isViewfinderActive = false;
                return;
            }

            // Press C to toggle viewfinder
            if (keyboard.cKey.wasPressedThisFrame)
            {
                _isViewfinderActive = !_isViewfinderActive;
                Debug.Log($"[Camera] Viewfinder toggled: {_isViewfinderActive}");
            }

            // Space or LMB while aiming → capture
            if (_isViewfinderActive)
            {
                bool spacePressed = keyboard.spaceKey.wasPressedThisFrame;
                bool lmbPressed   = mouse != null && mouse.leftButton.wasPressedThisFrame;

                if (spacePressed || lmbPressed)
                {
                    TryCapture();
                }
            }
        }

        // ─── Capture ──────────────────────────────────────────────────────
        private void TryCapture()
        {
            if (_cooldownTimer > 0)
            {
                Debug.Log($"[Camera] Cooldown: {_cooldownTimer:0.0}s remaining.");
                return;
            }

            _cooldownTimer = captureCooldown;
            _flashTimer    = FLASH_DURATION;

            Debug.Log("[Camera] SHUTTER: Click!");

            // Detect subject first (cheap raycast)
            var subject = DetectSubject();

            // Capture actual render via coroutine (end-of-frame)
            StartCoroutine(CaptureFrameCoroutine(subject));
        }

        private IEnumerator CaptureFrameCoroutine(GPOyun.Environment.PhotoSubject subject)
        {
            yield return new WaitForEndOfFrame();

            int w = Screen.width > 120 ? Screen.width / 2 : 640;
            int h = Screen.height > 120 ? Screen.height / 2 : 480;
            
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            bool captured = false;

            try
            {
                tex.ReadPixels(new Rect(Screen.width / 4, Screen.height / 4, w, h), 0, 0);
                tex.Apply();
                
                Color pixelSample = tex.GetPixel(w / 2, h / 2);
                if (pixelSample.r > 0.01f || pixelSample.g > 0.01f || pixelSample.b > 0.01f)
                {
                    captured = true;
                }
            }
            catch
            {
                captured = false;
            }

            if (!captured)
            {
                Camera cam = GetComponent<Camera>();
                if (cam == null) cam = Camera.main;
                if (cam != null)
                {
                    RenderTexture rt = RenderTexture.GetTemporary(w, h, 24);
                    RenderTexture previousActive = RenderTexture.active;
                    RenderTexture previousTarget = cam.targetTexture;

                    cam.targetTexture = rt;
                    cam.Render();

                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                    tex.Apply();

                    cam.targetTexture = previousTarget;
                    RenderTexture.active = previousActive;
                    RenderTexture.ReleaseTemporary(rt);
                    captured = true;
                }
            }

            if (!captured || tex == null)
            {
                tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float t = (float)y / h;
                        Color gradientColor = Color.Lerp(new Color(1f, 0.45f, 0.2f), new Color(0.12f, 0.25f, 0.7f), t);
                        tex.SetPixel(x, y, gradientColor);
                    }
                }
                tex.Apply();
            }

            PhotoData photoData = null;
            if (PhotoScorer.Instance != null)
            {
                photoData = PhotoScorer.Instance.ScorePhoto(GetComponent<Camera>(), subject, tex);
            }
            else
            {
                photoData = new PhotoData
                {
                    WorldPosition    = transform.position,
                    CapturedTexture  = tex,
                    PrimarySubject   = subject,
                    CompositionScore = subject != null ? subject.InterestLevel + 20 : Random.Range(5, 25)
                };
            }

            // Save PNG to disk!
            string subjName = subject != null ? subject.SubjectName : "Environmental";
            photoData.FilePath = SaveTextureToDisk(tex, subjName);

            // Hand over the photo data to our interactive PhotoReviewUI modal instead of auto-storing it!
            if (PhotoReviewUI.Instance != null)
            {
                PhotoReviewUI.Instance.ShowReview(photoData);
            }
            else
            {
                if (NewspaperManager.Instance != null)
                    NewspaperManager.Instance.StorePhoto(photoData);
                var gallery = FindAnyObjectByType<PhotoGalleryUI>();
                if (gallery != null) gallery.OnPhotoCaptured(photoData);
            }

            if (subject != null)
                Debug.Log($"[Camera] Captured & Saved: {subject.SubjectName} at {photoData.FilePath} (score: {photoData.CompositionScore})");
            else
                Debug.Log($"[Camera] Environmental shot saved at {photoData.FilePath} (score: {photoData.CompositionScore})");
        }

        private string SaveTextureToDisk(Texture2D tex, string subjectName)
        {
            try
            {
                // Ensure directory exists inside project Assets folder
                string folderPath = System.IO.Path.Combine(Application.dataPath, "_Game", "CapturedPhotos");
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                // Generate uuid-date-context naming convention
                string uuid = System.Guid.NewGuid().ToString().Substring(0, 8);
                string dateStr = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                // Sanitize context name to be safe for OS filesystems
                string cleanSubject = string.IsNullOrEmpty(subjectName) ? "Environmental" : subjectName;
                cleanSubject = System.Text.RegularExpressions.Regex.Replace(cleanSubject, @"[^a-zA-Z0-9_\-]", "");
                if (cleanSubject.Length > 20) cleanSubject = cleanSubject.Substring(0, 20);

                string filename = $"{uuid}_{dateStr}_{cleanSubject}.png";
                string fullPath = System.IO.Path.Combine(folderPath, filename);

                // Encode texture to PNG and write bytes
                byte[] bytes = tex.EncodeToPNG();
                System.IO.File.WriteAllBytes(fullPath, bytes);

                Debug.Log($"[Camera] Saved photo file to disk: {fullPath}");

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                return $"Assets/_Game/CapturedPhotos/{filename}";
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Camera] Exception saving photo: {ex.Message}");
                return string.Empty;
            }
        }

        // ─── Subject Detection ─────────────────────────────────────────────
        private GPOyun.Environment.PhotoSubject DetectSubject()
        {
            var cam = Camera.main ?? GetComponent<Camera>();
            if (cam == null) return null;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            int layerMask = ~((1 << 6) | (1 << 2)); // Ignore Ground (6) and Ignore Raycast (2)

            if (Physics.Raycast(ray, out RaycastHit hit, 30f, layerMask))
            {
                var s = hit.collider.GetComponentInParent<GPOyun.Environment.PhotoSubject>();
                if (s != null) return s;
            }
            return null;
        }

        // ─── State ────────────────────────────────────────────────────────
        public bool IsViewfinderActive() => _isViewfinderActive;
        public bool IsFlashing()         => _flashTimer > 0;
        public float FlashAlpha()        => _flashTimer / FLASH_DURATION;

        public void SetViewfinderExternal(bool active)
        {
            _isViewfinderActive = active;
            Debug.Log($"[Camera] External viewfinder request: {active}");
        }
    }
}
