using UnityEngine;
using UnityEngine.UI;
using GPOyun.Newspaper;
using GPOyun.Core;
using GPOyun.Managers;
using System.Collections.Generic;

namespace GPOyun.UI
{
    /// <summary>
    /// The "Tomorrow's Edition" interface.
    /// Appears in the Evening to let the player choose headlines.
    /// </summary>
    public class EditorialUI : MonoBehaviour
    {
        public static EditorialUI Instance { get; private set; }

        [Header("References")]
        public CanvasGroup editorialCanvasGroup;
        public RectTransform rollContainer;
        public Text statusText;

        private bool _isExplicitlyOpen = false;
        private bool _wasAutoOpenedToday = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Hide();
        }

        private void Update()
        {
            HandleInputs();
            CheckTimedAutoOpen();
        }

        private void HandleInputs()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.nKey.wasPressedThisFrame)
            {
                if (_isExplicitlyOpen) Hide();
                else Show();
            }
        }

        private void CheckTimedAutoOpen()
        {
            if (TimeManager.Instance == null) return;

            float hour = TimeManager.Instance.GetCurrentHour();
            
            if (hour >= 22f && !_wasAutoOpenedToday && !_isExplicitlyOpen)
            {
                _wasAutoOpenedToday = true;
                Show();
                if (statusText != null) statusText.text = "NIGHT SHIFT: SELECT TOMORROW'S HEADLINES";
            }

            if (hour >= 6f && hour < 7f) _wasAutoOpenedToday = false;
        }

        public void Show()
        {
            if (editorialCanvasGroup == null) return;
            
            SettingsController.Instance?.Hide();
            PhotoGalleryUI.Instance?.Hide();
            JournalUI.Instance?.Hide();

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.PauseGame();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            editorialCanvasGroup.alpha = 1f;
            editorialCanvasGroup.blocksRaycasts = true;
            editorialCanvasGroup.interactable = true;
            _isExplicitlyOpen = true;
            
            if (statusText != null) statusText.text = "EDITORIAL DESK";
            Debug.Log("[EditorialUI] Workspace active.");
        }

        public void Hide()
        {
            if (!_isExplicitlyOpen) return;
            _isExplicitlyOpen = false;

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.ResumeGame();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (editorialCanvasGroup != null)
            {
                editorialCanvasGroup.alpha = 0f;
                editorialCanvasGroup.blocksRaycasts = false;
                editorialCanvasGroup.interactable = false;
            }
        }

        // Must be wired in Inspector to a button
        public void OnPublishClicked()
        {
            Debug.Log("[EditorialUI] Selection confirmed.");
            
            var photos = NewspaperManager.Instance.GetTodaysPhotos();
            NewsCategory finalCat = NewsCategory.Local;
            string headline = "A Quiet Day";

            if (photos != null && photos.Count > 0)
            {
                var lastPhoto = photos[photos.Count - 1];
                if (lastPhoto.PrimarySubject != null)
                {
                    finalCat = lastPhoto.PrimarySubject.PrimaryCategory;
                    headline = $"New {finalCat} Event Captured!";
                }
            }

            NewsStory front = new NewsStory { Headline = headline, Category = finalCat };
            NewspaperManager.Instance.PublishEdition(front);
            
            Hide();
        }
    }
}
