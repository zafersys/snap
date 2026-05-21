using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GPOyun.Newspaper;

namespace GPOyun.UI
{
    /// <summary>
    /// Full-screen photo gallery / lightbox.
    /// Appears when the player presses [G] or when Editorial Desk auto-opens at night.
    /// Shows thumbnail cards of all captured photos, a selected zoom view, and a Publish button.
    /// </summary>
    public class PhotoGalleryUI : MonoBehaviour
    {
        private static PhotoGalleryUI _instance;
        public static PhotoGalleryUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<PhotoGalleryUI>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [Header("UI References — built procedurally")]
        public CanvasGroup galleryGroup;
        public RectTransform thumbnailRow;
        public RawImage      zoomImage;
        public Text          metaLabel;
        public Text          scoreLabel;
        public Button        publishButton;
        public Text          publishButtonText;

        private bool         _isOpen;
        public bool IsOpen => _isOpen;
        private PhotoData    _selectedPhoto;
        private readonly List<RawImage> _thumbImages = new();

        private void Awake()
        {
            _instance = this;
        }

        private void Start() => Hide();

        // Input listening handled centrally by GlobalInputListener

        // ─── Lifecycle ────────────────────────────────────────────────────

        public void Toggle() { if (_isOpen) Hide(); else Show(); }

        public void Show()
        {
            _isOpen = true;
            SettingsController.Instance?.Hide();
            JournalUI.Instance?.Hide();
            EditorialUI.Instance?.Hide();
            NewspaperBoardUI.Instance?.Hide();

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.PauseGame();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            Apply(1f, true, true);
            RefreshThumbnails();
            Debug.Log("[Gallery] Opened.");
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.ResumeGame();

            Apply(0f, false, false);
            Debug.Log("[Gallery] Closed.");
        }

        private Coroutine _fadeCoroutine;

        private void Apply(float targetAlpha, bool blocks, bool interact)
        {
            if (galleryGroup == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (Application.isBatchMode)
            {
                galleryGroup.alpha          = targetAlpha;
                galleryGroup.blocksRaycasts = blocks;
                galleryGroup.interactable   = interact;
            }
            else
            {
                _fadeCoroutine = StartCoroutine(GPOyun.Core.VisualUtils.FadeGroupCoroutine(galleryGroup, targetAlpha, 0.15f, blocks, interact));
            }
        }

        // ─── Photo Notification ───────────────────────────────────────────

        public void OnPhotoCaptured(PhotoData photo)
        {
            // Auto-select the newest photo
            _selectedPhoto = photo;
            if (_isOpen) RefreshThumbnails();
        }

        // ─── Thumbnail Row ────────────────────────────────────────────────

        private void RefreshThumbnails()
        {
            if (thumbnailRow == null) return;

            var photos = NewspaperManager.Instance?.GetTodaysPhotos();
            if (photos == null) return;

            // Clear existing
            foreach (Transform child in thumbnailRow) Destroy(child.gameObject);
            _thumbImages.Clear();

            if (photos.Count == 0)
            {
                SetMeta(null);
                if (zoomImage != null) zoomImage.texture = null;
                return;
            }

            for (int i = 0; i < photos.Count; i++)
            {
                var photo  = photos[i];
                int idx    = i; // capture for lambda

                // Card
                var card = new GameObject($"ThumbCard_{i}");
                card.transform.SetParent(thumbnailRow, false);
                var cardRect = card.AddComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(130, 100);

                // Background
                var bg = card.AddComponent<Image>();
                bg.color = _selectedPhoto == photo
                    ? new Color(0.9f, 0.75f, 0.3f, 1f)  // gold border if selected
                    : new Color(0.1f, 0.1f, 0.15f, 0.8f);

                // Thumbnail image
                var imgGo = new GameObject("Thumb");
                imgGo.transform.SetParent(card.transform, false);
                var imgRect = imgGo.AddComponent<RectTransform>();
                imgRect.anchorMin = new Vector2(0.05f, 0.15f);
                imgRect.anchorMax = new Vector2(0.95f, 0.95f);
                imgRect.offsetMin = imgRect.offsetMax = Vector2.zero;
                var rawImg = imgGo.AddComponent<RawImage>();
                rawImg.texture = photo.CapturedTexture;
                _thumbImages.Add(rawImg);

                // Score badge
                var badgeGo = new GameObject("Score");
                badgeGo.transform.SetParent(card.transform, false);
                var badgeRect = badgeGo.AddComponent<RectTransform>();
                badgeRect.anchorMin  = new Vector2(0, 0);
                badgeRect.anchorMax  = new Vector2(1, 0.15f);
                badgeRect.offsetMin  = badgeRect.offsetMax = Vector2.zero;
                var badgeTxt = badgeGo.AddComponent<Text>();
                badgeTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                badgeTxt.fontSize  = 14;
                badgeTxt.color     = Color.white;
                badgeTxt.text      = $"★ {photo.CompositionScore}";
                badgeTxt.alignment = TextAnchor.MiddleCenter;

                // Click to select
                var btn = card.AddComponent<Button>();
                btn.onClick.AddListener(() => SelectPhoto(photos[idx]));
            }

            // Auto-select the last photo
            if (_selectedPhoto == null && photos.Count > 0)
                _selectedPhoto = photos[photos.Count - 1];

            if (_selectedPhoto != null) SetZoom(_selectedPhoto);
        }

        public void SelectPhoto(PhotoData photo)
        {
            _selectedPhoto = photo;
            SetZoom(photo);
            RefreshThumbnails(); // re-colour borders
        }

        private void SetZoom(PhotoData photo)
        {
            if (zoomImage != null) zoomImage.texture = photo.CapturedTexture;
            SetMeta(photo);
        }

        private void SetMeta(PhotoData photo)
        {
            if (metaLabel != null)
            {
                if (photo == null) { metaLabel.text = "No photos today."; return; }
                string subj = photo.PrimarySubject != null ? photo.PrimarySubject.SubjectName : "Environment";
                string cat  = photo.PrimarySubject != null ? photo.PrimarySubject.PrimaryCategory.ToString() : "–";
                metaLabel.text = $"Subject: {subj}\nCategory: {cat}";
            }

            if (scoreLabel != null && photo != null)
                scoreLabel.text = $"Score  ★ {photo.CompositionScore} / 100";
        }

        // ─── Publish ──────────────────────────────────────────────────────

        /// <summary>Called by the Publish button. Selects the best photo and publishes.</summary>
        public void OnPublishClicked()
        {
            var photos = NewspaperManager.Instance?.GetTodaysPhotos();
            if (photos == null || photos.Count == 0)
            {
                Debug.Log("[Gallery] Nothing to publish — go take photos first!");
                if (metaLabel != null) metaLabel.text = "Go take photos first!";
                return;
            }

            // Pick best-scoring photo
            PhotoData best = _selectedPhoto ?? photos[0];
            foreach (var p in photos)
                if (p.CompositionScore > best.CompositionScore) best = p;

            string headline    = GenerateHeadline(best);
            NewsCategory cat   = best.PrimarySubject?.PrimaryCategory ?? NewsCategory.Local;
            NewsStory frontPage = new NewsStory
            {
                Headline = headline,
                Photo    = best.CapturedTexture,
                Category = cat,
                SourcePhoto = best
            };

            NewspaperManager.Instance.PublishEdition(frontPage);
            Debug.Log($"[Gallery] Published: \"{headline}\"");

            if (publishButtonText != null) publishButtonText.text = "PUBLISHED!";
            Hide();

            if (NewspaperBoardUI.Instance != null)
            {
                NewspaperBoardUI.Instance.Show();
            }
        }

        private string GenerateHeadline(PhotoData photo)
        {
            if (photo.PrimarySubject == null)
                return "A Quiet Day In Town";

            string subj = photo.PrimarySubject.SubjectName;
            NewsCategory cat = photo.PrimarySubject.PrimaryCategory;

            return (cat, photo.CompositionScore) switch
            {
                (NewsCategory.Scandal,     > 70) => $"EXCLUSIVE: {subj} — Scandal Rocks The Square!",
                (NewsCategory.Celebration, > 70) => $"{subj} Celebrations Fill The Town With Joy",
                (NewsCategory.Disaster,    > 60) => $"Breaking: {subj} — Town In Shock",
                (NewsCategory.Global,      > 50) => $"{subj}: Eyes On The World",
                _                                => $"Town Life: {subj} Spotted In The Square"
            };
        }
    }
}
