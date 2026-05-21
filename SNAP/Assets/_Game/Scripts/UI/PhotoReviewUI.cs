using System.IO;
using UnityEngine;
using UnityEngine.UI;
using GPOyun.Newspaper;
using GPOyun.UseCases;
using GPOyun.Core;

namespace GPOyun.UI
{
    /// <summary>
    /// Steve Jobs-inspired high-density minimalist UI review panel for captured snapshots.
    /// Provides Keep, Take New, Delete, and instant Publish triggers.
    /// </summary>
    public class PhotoReviewUI : MonoBehaviour
    {
        public static PhotoReviewUI Instance { get; private set; }

        private bool _isOpen = false;
        private PhotoData _currentPhoto;

        private CanvasGroup _canvasGroup;
        private RawImage _previewImage;
        private Text _scoreLabel;
        private Text _metaLabel;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
        }

        public void ShowReview(PhotoData photo)
        {
            _currentPhoto = photo;
            _isOpen = true;

            if (GameManager.Instance != null) GameManager.Instance.PauseGame();

            if (_previewImage != null) _previewImage.texture = photo.CapturedTexture;
            
            string subjectName = photo.PrimarySubject != null ? photo.PrimarySubject.SubjectName : "Scenic Landscape";
            string categoryName = photo.PrimarySubject != null ? photo.PrimarySubject.PrimaryCategory.ToString() : "Environmental";
            
            if (_metaLabel != null) _metaLabel.text = $"🎯 SUBJECT: {subjectName.ToUpper()}  |  🎭 TYPE: {categoryName.ToUpper()}";
            if (_scoreLabel != null) _scoreLabel.text = $"★ COMPOSITION: {photo.CompositionScore} / 100";

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }

            // Unlock cursor so user can click review buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            // Re-lock cursor to return to walking mode
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void KeepPhoto()
        {
            Debug.Log("[PhotoReviewUI] Keep clicked.");
            if (_currentPhoto != null)
            {
                if (NewspaperManager.Instance != null)
                {
                    NewspaperManager.Instance.StorePhoto(_currentPhoto);
                }
                if (PhotoGalleryUI.Instance != null)
                {
                    PhotoGalleryUI.Instance.OnPhotoCaptured(_currentPhoto);
                }
            }
            Hide();
        }

        private void RetakePhoto()
        {
            Debug.Log("[PhotoReviewUI] Retake clicked. Re-opening viewfinder.");
            DeletePhysicalFile();
            Hide();

            // Set camera viewfinder state active immediately to allow rapid retakes
            var camCtrl = FindAnyObjectByType<CameraSystem.CameraController>();
            if (camCtrl != null)
            {
                camCtrl.SetViewfinderExternal(true);
            }
        }

        private void DeletePhoto()
        {
            Debug.Log("[PhotoReviewUI] Delete clicked. Discarding photo.");
            DeletePhysicalFile();
            Hide();
        }

        private void PublishPhoto()
        {
            Debug.Log("[PhotoReviewUI] Publish clicked. Triggering immediate publication!");
            if (_currentPhoto != null)
            {
                // Sync store
                if (NewspaperManager.Instance != null)
                {
                    NewspaperManager.Instance.StorePhoto(_currentPhoto);
                }
                if (PhotoGalleryUI.Instance != null)
                {
                    PhotoGalleryUI.Instance.OnPhotoCaptured(_currentPhoto);
                }

                // Execute Compose Editorial Use Case
                if (ComposeEditorialUseCase.Instance != null)
                {
                    ComposeEditorialUseCase.Instance.Execute(_currentPhoto);
                }

                // Instantly open the physical Newspaper Board so they see it
                if (NewspaperBoardUI.Instance != null)
                {
                    NewspaperBoardUI.Instance.Show();
                }
            }
            Hide();
        }

        private void DeletePhysicalFile()
        {
            if (_currentPhoto == null || string.IsNullOrEmpty(_currentPhoto.FilePath)) return;

            try
            {
                string fullPath = _currentPhoto.FilePath;
                if (!Path.IsPathRooted(fullPath))
                {
                    fullPath = Path.Combine(Application.dataPath, "..", _currentPhoto.FilePath);
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Debug.Log($"[PhotoReviewUI] Physically deleted file: {fullPath}");
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[PhotoReviewUI] Discard cleanup failed: {ex.Message}");
            }
        }

        private void BuildUI()
        {
            // Canvas setup
            Canvas canvas = VisualUtils.CreateBaseCanvas("MOCK_PHOTO_REVIEW", 110, transform);
            _canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            // Muted dark backing block
            GameObject bg = new GameObject("ReviewBacking");
            bg.transform.SetParent(canvas.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.06f, 0.85f); // Slightly more transparent background to see the game world behind it!
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Frame Holder (Exactly 2x smaller compact polaroid: 360x380)
            GameObject frame = new GameObject("ReviewFrame");
            frame.transform.SetParent(canvas.transform, false);
            var frameRect = frame.AddComponent<RectTransform>();
            frameRect.sizeDelta = new Vector2(360, 380);
            frameRect.anchoredPosition = new Vector2(0, 10);

            var frameImg = frame.AddComponent<Image>();
            frameImg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
            var outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.25f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Preview Render Image (Compact: 320x200)
            GameObject imgGo = new GameObject("PreviewRawImage");
            imgGo.transform.SetParent(frame.transform, false);
            _previewImage = imgGo.AddComponent<RawImage>();
            var previewRect = imgGo.GetComponent<RectTransform>();
            previewRect.sizeDelta = new Vector2(320, 200);
            previewRect.anchoredPosition = new Vector2(0, 55);

            // Meta Info label (Compact font size)
            GameObject metaGo = new GameObject("MetaText");
            metaGo.transform.SetParent(frame.transform, false);
            _metaLabel = metaGo.AddComponent<Text>();
            _metaLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _metaLabel.fontSize = 11;
            _metaLabel.alignment = TextAnchor.MiddleCenter;
            _metaLabel.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            var metaRect = metaGo.GetComponent<RectTransform>();
            metaRect.anchoredPosition = new Vector2(0, -65);
            metaRect.sizeDelta = new Vector2(320, 25);

            // Score label (Compact font size)
            GameObject scoreGo = new GameObject("ScoreText");
            scoreGo.transform.SetParent(frame.transform, false);
            _scoreLabel = scoreGo.AddComponent<Text>();
            _scoreLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _scoreLabel.fontSize = 15;
            _scoreLabel.fontStyle = FontStyle.Bold;
            _scoreLabel.alignment = TextAnchor.MiddleCenter;
            _scoreLabel.color = VisualUtils.Terracotta;
            var scoreRect = scoreGo.GetComponent<RectTransform>();
            scoreRect.anchoredPosition = new Vector2(0, -95);
            scoreRect.sizeDelta = new Vector2(320, 25);

            // --- BUTTONS CONTAINER WITH HORIZONTAL LAYOUT GROUP ---
            GameObject btnGroup = new GameObject("ButtonGroup");
            btnGroup.transform.SetParent(frame.transform, false);
            var groupRect = btnGroup.AddComponent<RectTransform>();
            groupRect.anchoredPosition = new Vector2(0, -145);
            groupRect.sizeDelta = new Vector2(330, 40);

            var layout = btnGroup.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(6, 6, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Create 4 buttons: Keep, Retake, Delete, Publish with premium compact gaming HSL colors
            CreateReviewButton("KeepBtn", "KEEP (SAVE)", new Color(0.95f, 0.95f, 0.98f), new Color(0.08f, 0.08f, 0.12f), KeepPhoto, btnGroup.transform);
            CreateReviewButton("RetakeBtn", "TAKE NEW", new Color(0.22f, 0.45f, 0.65f), Color.white, RetakePhoto, btnGroup.transform);
            CreateReviewButton("DeleteBtn", "DELETE", new Color(0.85f, 0.28f, 0.28f), Color.white, DeletePhoto, btnGroup.transform);
            CreateReviewButton("PublishBtn", "PUBLISH NOW", new Color(0.18f, 0.52f, 0.32f), Color.white, PublishPhoto, btnGroup.transform);

            VisualUtils.EnsureCanvasRenderers(transform);
        }

        private void CreateReviewButton(string goName, string btnText, Color bgCol, Color txtCol, System.Action callback, Transform parent)
        {
            GameObject btnGo = new GameObject(goName);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();

            var img = btnGo.AddComponent<Image>();
            img.color = bgCol;

            var outline = btnGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.2f);
            outline.effectDistance = new Vector2(1f, 1f);

            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => callback());

            // Label
            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);
            var txt = textGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 9; // Compact font to fit beautifully in 2x smaller passport button layout
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = btnText;
            txt.color = txtCol;

            var txtRect = textGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
        }
    }
}
