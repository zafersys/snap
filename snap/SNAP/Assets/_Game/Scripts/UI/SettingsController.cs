using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace GPOyun.UI
{
    /// <summary>
    /// Settings panel — owns the ESC key exclusively.
    /// ESC toggles the panel open/close and manages cursor lock state.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        private static SettingsController _instance;
        public static SettingsController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<SettingsController>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [SerializeField] private CanvasGroup settingsCanvasGroup;
        private bool _isOpen = false;
        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _instance = this;
        }

        public void Initialize(CanvasGroup cg)
        {
            settingsCanvasGroup = cg;
            SetupListeners();
        }

        private void SetupListeners()
        {
            if (settingsCanvasGroup == null) return;

            Slider[] sliders = settingsCanvasGroup.GetComponentsInChildren<Slider>(true);
            foreach (var s in sliders)
            {
                if (s.gameObject.name.Contains("Sensitivity"))
                {
                    s.value = 0.5f;
                    s.onValueChanged.AddListener(val =>
                    {
                        var player = Object.FindAnyObjectByType<GPOyun.Player.PlayerController>();
                        if (player != null)
                        {
                            player.moveSpeed = 3f + val * 6f;    // 3–9
                            player.rotationSpeed = 60f + val * 120f; // 60–180
                        }
                    });
                }
                else if (s.gameObject.name.Contains("Volume"))
                {
                    s.value = 1f;
                    s.onValueChanged.AddListener(val =>
                    {
                        AudioListener.volume = val;
                    });
                }
            }
        }

        // Input listening handled centrally by GlobalInputListener

        private void OpenSettings()
        {
            _isOpen = true;
            PhotoGalleryUI.Instance?.Hide();
            JournalUI.Instance?.Hide();
            EditorialUI.Instance?.Hide();
            NewspaperBoardUI.Instance?.Hide();

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.PauseGame();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            Apply(1f, true, true);
            Debug.Log("[Settings] Opened.");
        }

        private void CloseSettings()
        {
            _isOpen = false;
            if (Core.GameManager.Instance != null) Core.GameManager.Instance.ResumeGame();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            Apply(0f, false, false);
            Debug.Log("[Settings] Closed.");
        }

        public void Hide()
        {
            if (_isOpen) CloseSettings();
        }

        public void ToggleSettings()
        {
            if (_isOpen) CloseSettings();
            else         OpenSettings();
        }

        private Coroutine _fadeCoroutine;

        private void Apply(float alpha, bool blocksRay, bool interactable)
        {
            if (settingsCanvasGroup == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (Application.isBatchMode)
            {
                settingsCanvasGroup.alpha          = alpha;
                settingsCanvasGroup.blocksRaycasts = blocksRay;
                settingsCanvasGroup.interactable   = interactable;
            }
            else
            {
                _fadeCoroutine = StartCoroutine(GPOyun.Core.VisualUtils.FadeGroupCoroutine(settingsCanvasGroup, alpha, 0.15f, blocksRay, interactable));
            }
        }
    }
}
