using UnityEngine;
using UnityEngine.InputSystem;
using GPOyun.UI;
using GPOyun.Core;

namespace GPOyun.InputSystem
{
    /// <summary>
    /// High-reliability centralized Input Listener that manages toggling of all game overlays.
    /// This bypasses component focus/inactive updating issues by acting as the single source of truth.
    /// </summary>
    public class GlobalInputListener : MonoBehaviour
    {
        public static GlobalInputListener Instance { get; private set; }

        private bool _escWasPressed = false;
        private bool _gWasPressed = false;
        private bool _jWasPressed = false;
        private bool _bWasPressed = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            bool escapePressedThisFrame = keyboard.escapeKey?.wasPressedThisFrame ?? false;

            // Gracefully yield key controls if the Photo Review modal is active
            if (PhotoReviewUI.Instance != null && PhotoReviewUI.Instance.IsOpen)
            {
                if (escapePressedThisFrame)
                {
                    PhotoReviewUI.Instance.Hide();
                }
                return;
            }

            // Diagnostic: Log any key pressed
            foreach (var key in keyboard.allKeys)
            {
                if (key == null) continue; // 54. satırdaki NullReference hatasını çözen kısım

                if (key.wasPressedThisFrame)
                {
                    Debug.Log($"[GlobalInputListener] Diagnostic - Key pressed: {key.name}");
                }
            }

            // ── ESC KEY (Close all panels or toggle settings) ───────────────────────────────────
            bool escPressed = keyboard.escapeKey?.isPressed ?? false;
            if (escPressed && !_escWasPressed)
            {
                _escWasPressed = true;
                Debug.Log("[GlobalInputListener] ESC pressed!");

                bool closedAny = false;

                if (PhotoGalleryUI.Instance != null && PhotoGalleryUI.Instance.IsOpen)
                {
                    PhotoGalleryUI.Instance.Hide();
                    closedAny = true;
                }
                if (JournalUI.Instance != null && JournalUI.Instance.IsOpen)
                {
                    JournalUI.Instance.Hide();
                    closedAny = true;
                }
                if (NewspaperBoardUI.Instance != null && NewspaperBoardUI.Instance.IsOpen)
                {
                    NewspaperBoardUI.Instance.Hide();
                    closedAny = true;
                }
                if (SettingsController.Instance != null && SettingsController.Instance.IsOpen)
                {
                    SettingsController.Instance.Hide();
                    closedAny = true;
                }

                if (!closedAny)
                {
                    if (SettingsController.Instance == null)
                    {
                        Debug.Log("[GlobalInputListener] SettingsController is missing. Spawning dynamic self-healing fallback!");
                        var settingsGo = new GameObject("SettingsController");
                        var controller = settingsGo.AddComponent<SettingsController>();
                        VisualUtils.SetupMockSettings(controller);
                    }
                    SettingsController.Instance.ToggleSettings();
                }
            }
            else if (!escPressed)
            {
                _escWasPressed = false;
            }

            // ── G KEY (Photo Gallery Screen) ────────────────────────────────
            bool gPressed = keyboard.gKey?.isPressed ?? false;
            if (gPressed && !_gWasPressed)
            {
                _gWasPressed = true;
                Debug.Log("[GlobalInputListener] G pressed - Toggling Photo Gallery!");
                if (PhotoGalleryUI.Instance == null)
                {
                    Debug.Log("[GlobalInputListener] PhotoGalleryUI is missing. Spawning dynamic self-healing fallback!");
                    var galleryGo = new GameObject("PhotoGalleryUI");
                    var gallery = galleryGo.AddComponent<PhotoGalleryUI>();
                    VisualUtils.SetupPhotoGallery(gallery);
                }
                PhotoGalleryUI.Instance.Toggle();
            }
            else if (!gPressed)
            {
                _gWasPressed = false;
            }

            // ── J KEY (Observational Journal Screen) ─────────────────────────
            bool jPressed = keyboard.jKey?.isPressed ?? false;
            if (jPressed && !_jWasPressed)
            {
                _jWasPressed = true;
                Debug.Log("[GlobalInputListener] J pressed - Toggling Journal!");
                if (JournalUI.Instance == null)
                {
                    Debug.Log("[GlobalInputListener] JournalUI is missing. Spawning dynamic self-healing fallback!");
                    var jUiGo = new GameObject("JournalUI");
                    jUiGo.AddComponent<JournalUI>();
                }
                JournalUI.Instance.Toggle();
            }
            else if (!jPressed)
            {
                _jWasPressed = false;
            }

            // ── B KEY (Newspaper Board UI) ───────────────────────────────────
            bool bPressed = keyboard.bKey?.isPressed ?? false;
            if (bPressed && !_bWasPressed)
            {
                _bWasPressed = true;
                Debug.Log("[GlobalInputListener] B pressed - Toggling Newspaper Board panel!");
                if (NewspaperBoardUI.Instance == null)
                {
                    Debug.Log("[GlobalInputListener] NewspaperBoardUI is missing. Spawning dynamic self-healing fallback!");
                    var boardUiGo = new GameObject("NewspaperBoardUI");
                    boardUiGo.AddComponent<NewspaperBoardUI>();
                }
                NewspaperBoardUI.Instance.Toggle();
            }
            else if (!bPressed)
            {
                _bWasPressed = false;
            }
        }
    }
}