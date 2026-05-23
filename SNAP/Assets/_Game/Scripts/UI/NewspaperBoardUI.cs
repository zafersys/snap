using UnityEngine;
using UnityEngine.UI;
using GPOyun.Core;
using GPOyun.Newspaper;

namespace GPOyun.UI
{
    /// <summary>
    /// Cozy fullscreen Bulletin Board overlay. Opens with [B] key.
    /// Procedurally displays the latest published daily edition of the town square newspaper.
    /// Built completely procedurally (Zero-Asset) to ensure seamless setup.
    /// </summary>
    public class NewspaperBoardUI : MonoBehaviour
    {
        private static NewspaperBoardUI _instance;
        public static NewspaperBoardUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<NewspaperBoardUI>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [Header("References")]
        public CanvasGroup boardCanvasGroup;
        public Text headlineText;
        public Text categoryText;
        public RawImage photoImage;
        public Text emptyMessageText;

        private bool _isOpen = false;
        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void Start()
        {
            SetupUI();
            Hide();
        }

        public void Toggle()
        {
            if (_isOpen) Hide();
            else Show();
        }

        public void Show()
        {
            _isOpen = true;
            SettingsController.Instance?.Hide();
            PhotoGalleryUI.Instance?.Hide();
            JournalUI.Instance?.Hide();
            EditorialUI.Instance?.Hide();

            if (GameManager.Instance != null) GameManager.Instance.PauseGame();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            RefreshDisplay();
            Apply(1f, true, true);
            Debug.Log("[NewspaperBoardUI] Opened.");
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Apply(0f, false, false);
            Debug.Log("[NewspaperBoardUI] Closed.");
        }

        private Coroutine _fadeCoroutine;

        private void Apply(float targetAlpha, bool blocks, bool interact)
        {
            if (boardCanvasGroup == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (Application.isBatchMode)
            {
                boardCanvasGroup.alpha = targetAlpha;
                boardCanvasGroup.blocksRaycasts = blocks;
                boardCanvasGroup.interactable = interact;
            }
            else
            {
                _fadeCoroutine = StartCoroutine(VisualUtils.FadeGroupCoroutine(boardCanvasGroup, targetAlpha, 0.15f, blocks, interact));
            }
        }

        public void RefreshDisplay()
        {
            if (NewspaperManager.Instance == null) return;
            var history = NewspaperManager.Instance.GetHistory();

            if (history == null || history.Count == 0)
            {
                headlineText.gameObject.SetActive(false);
                categoryText.gameObject.SetActive(false);
                photoImage.gameObject.SetActive(false);
                emptyMessageText.gameObject.SetActive(true);
                emptyMessageText.text = "THE BOARD IS EMPTY.\n\nNo edition has been published yet today.\nTake photos, capture high-interest subjects, and publish your paper from the gallery!";
            }
            else
            {
                var latestEdition = history[history.Count - 1];
                emptyMessageText.gameObject.SetActive(false);
                headlineText.gameObject.SetActive(true);
                categoryText.gameObject.SetActive(true);

                headlineText.text = latestEdition.FrontPage.Headline.ToUpper();
                categoryText.text = $"EDITION #{latestEdition.DayIndex} | CATEGORY: {latestEdition.FrontPage.Category.ToString().ToUpper()}";

                if (latestEdition.FrontPage.Photo != null)
                {
                    photoImage.gameObject.SetActive(true);
                    photoImage.texture = latestEdition.FrontPage.Photo;
                }
                else
                {
                    photoImage.gameObject.SetActive(false);
                }
            }
        }

        private void SetupUI()
        {
            Canvas canvas = VisualUtils.CreateBaseCanvas("BOARD_CANVAS", 850, transform); // Render on top of ordinary panels
            boardCanvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            boardCanvasGroup.alpha = 0f;
            boardCanvasGroup.blocksRaycasts = false;
            boardCanvasGroup.interactable = false;

            // Dark semi-transparent background overlay
            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.06f, 0.94f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // Headline board container (Placard)
            GameObject boardPlacard = new GameObject("BoardPlacard");
            boardPlacard.transform.SetParent(canvas.transform, false);
            var placardRect = boardPlacard.AddComponent<RectTransform>();
            placardRect.anchorMin = new Vector2(0.2f, 0.1f);
            placardRect.anchorMax = new Vector2(0.8f, 0.9f);
            placardRect.offsetMin = placardRect.offsetMax = Vector2.zero;

            // Outer Terracotta wood border
            var borderImg = boardPlacard.AddComponent<Image>();
            borderImg.color = VisualUtils.Terracotta;

            // Inner cream stucco paper background
            GameObject paperInner = new GameObject("PaperInner");
            paperInner.transform.SetParent(boardPlacard.transform, false);
            var paperRect = paperInner.AddComponent<RectTransform>();
            paperRect.anchorMin = new Vector2(0.02f, 0.02f);
            paperRect.anchorMax = new Vector2(0.98f, 0.98f);
            paperRect.offsetMin = paperRect.offsetMax = Vector2.zero;
            var paperImg = paperInner.AddComponent<Image>();
            paperImg.color = VisualUtils.StuccoWhite;

            // Newspaper Main Title Header
            GameObject newspaperHeader = new GameObject("NewspaperHeader");
            newspaperHeader.transform.SetParent(paperInner.transform, false);
            var headerRect = newspaperHeader.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.05f, 0.85f);
            headerRect.anchorMax = new Vector2(0.95f, 0.95f);
            headerRect.offsetMin = headerRect.offsetMax = Vector2.zero;
            var headerTxt = newspaperHeader.AddComponent<Text>();
            headerTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerTxt.fontSize = 32;
            headerTxt.fontStyle = FontStyle.Bold;
            headerTxt.color = Color.black;
            headerTxt.text = "THE DAILY TOWN SQUARE";
            headerTxt.alignment = TextAnchor.MiddleCenter;

            // Underline under title
            GameObject line = new GameObject("TitleLine");
            line.transform.SetParent(paperInner.transform, false);
            var lineRect = line.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.05f, 0.83f);
            lineRect.anchorMax = new Vector2(0.95f, 0.84f);
            lineRect.offsetMin = lineRect.offsetMax = Vector2.zero;
            var lineImg = line.AddComponent<Image>();
            lineImg.color = Color.black;

            // Category & Edition info line
            GameObject categoryGo = new GameObject("CategoryText");
            categoryGo.transform.SetParent(paperInner.transform, false);
            var catRect = categoryGo.AddComponent<RectTransform>();
            catRect.anchorMin = new Vector2(0.05f, 0.77f);
            catRect.anchorMax = new Vector2(0.95f, 0.82f);
            catRect.offsetMin = catRect.offsetMax = Vector2.zero;
            categoryText = categoryGo.AddComponent<Text>();
            categoryText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            categoryText.fontSize = 16;
            categoryText.color = Color.gray;
            categoryText.alignment = TextAnchor.MiddleCenter;

            // Headline Display
            GameObject headlineGo = new GameObject("HeadlineText");
            headlineGo.transform.SetParent(paperInner.transform, false);
            var headRect = headlineGo.AddComponent<RectTransform>();
            headRect.anchorMin = new Vector2(0.05f, 0.62f);
            headRect.anchorMax = new Vector2(0.95f, 0.75f);
            headRect.offsetMin = headRect.offsetMax = Vector2.zero;
            headlineText = headlineGo.AddComponent<Text>();
            headlineText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headlineText.fontSize = 24;
            headlineText.fontStyle = FontStyle.Bold;
            headlineText.color = Color.black;
            headlineText.alignment = TextAnchor.MiddleCenter;

            // Photo frame (Cobalt Blue accented container)
            GameObject photoFrame = new GameObject("PhotoFrame");
            photoFrame.transform.SetParent(paperInner.transform, false);
            var frameRect = photoFrame.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.15f, 0.15f);
            frameRect.anchorMax = new Vector2(0.85f, 0.60f);
            frameRect.offsetMin = frameRect.offsetMax = Vector2.zero;
            var frameImg = photoFrame.AddComponent<Image>();
            frameImg.color = VisualUtils.CobaltBlue;

            // Photo RawImage
            GameObject photoGo = new GameObject("Photo");
            photoGo.transform.SetParent(photoFrame.transform, false);
            var photoRect = photoGo.AddComponent<RectTransform>();
            photoRect.anchorMin = new Vector2(0.02f, 0.02f);
            photoRect.anchorMax = new Vector2(0.98f, 0.98f);
            photoRect.offsetMin = photoRect.offsetMax = Vector2.zero;
            photoImage = photoGo.AddComponent<RawImage>();
            photoImage.color = Color.white;

            // Empty state placeholder message
            GameObject emptyGo = new GameObject("EmptyMessageText");
            emptyGo.transform.SetParent(paperInner.transform, false);
            var emptyRect = emptyGo.AddComponent<RectTransform>();
            emptyRect.anchorMin = new Vector2(0.05f, 0.2f);
            emptyRect.anchorMax = new Vector2(0.95f, 0.65f);
            emptyRect.offsetMin = emptyRect.offsetMax = Vector2.zero;
            emptyMessageText = emptyGo.AddComponent<Text>();
            emptyMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyMessageText.fontSize = 20;
            emptyMessageText.color = Color.darkGray;
            emptyMessageText.alignment = TextAnchor.MiddleCenter;

            // Bottom close prompt instructions
            GameObject closeGo = new GameObject("ClosePromptText");
            closeGo.transform.SetParent(paperInner.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.05f, 0.08f);
            closeRect.anchorMax = new Vector2(0.95f, 0.13f);
            closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;
            var closeTxt = closeGo.AddComponent<Text>();
            closeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeTxt.fontSize = 14;
            closeTxt.color = Color.gray;
            closeTxt.text = "Press [B] or [ESC] to close and return to the square";
            closeTxt.alignment = TextAnchor.MiddleCenter;

            // Interactive Close Button for quick clicking!
            GameObject closeBtnGo = new GameObject("CloseButton");
            closeBtnGo.transform.SetParent(paperInner.transform, false);
            var btnRect = closeBtnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.35f, 0.02f);
            btnRect.anchorMax = new Vector2(0.65f, 0.07f);
            btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;
            var btnImg = closeBtnGo.AddComponent<Image>();
            btnImg.color = VisualUtils.Terracotta;
            var btnOutline = closeBtnGo.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0, 0, 0, 0.2f);
            
            var btn = closeBtnGo.AddComponent<Button>();
            btn.onClick.AddListener(Hide);

            GameObject btnTxtGo = new GameObject("Label");
            btnTxtGo.transform.SetParent(closeBtnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 14;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.color = Color.white;
            btnTxt.text = "CLOSE BOARD";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            var txtRect = btnTxtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;

            VisualUtils.EnsureCanvasRenderers(transform);
        }
    }
}
