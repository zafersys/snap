using UnityEngine;
using UnityEngine.UI;

namespace GPOyun.Core
{
    /// <summary>
    /// Centralized aesthetic utility for the Mediterranean Minimalist 'Zero-Asset' project.
    /// Handles URP material generation and provides the curated color palette.
    /// </summary>
    public static class VisualUtils
    {
        // --- Color Tokens (Mediterranean Palette) ---
        public static readonly Color StuccoWhite   = new Color(0.98f, 0.95f, 0.92f); // Primary walls
        public static readonly Color Terracotta     = new Color(0.85f, 0.35f, 0.2f); // Roofs and trims
        public static readonly Color CobaltBlue     = new Color(0.05f, 0.3f, 0.7f);  // Player and accent doors
        public static readonly Color CrimsonRed     = new Color(0.8f, 0.1f, 0.1f);   // Main Player (Distinct)
        public static readonly Color SlateGrey      = new Color(0.35f, 0.35f, 0.4f); // Ground
        public static readonly Color FountainBlue   = new Color(0.4f, 0.7f, 0.95f);  // Water
        public static readonly Color WoodBrown      = new Color(0.4f, 0.25f, 0.15f); // Info boards
        public static readonly Color PineGreen      = new Color(0.1f, 0.35f, 0.2f);  // Mediterranean Trees
        public static readonly Color OliveGreen     = new Color(0.35f, 0.45f, 0.25f); // Bushes

        private static Shader _urpShader;

        /// <summary>
        /// Applies a specific color and ensures a URP-compatible material is used.
        /// This is the 'Nuclear Cure' for pink objects.
        /// </summary>
        public static void ApplyAesthetic(GameObject obj, Color color, float smoothness = 0.2f)
        {
            if (_urpShader == null)
            {
                _urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (_urpShader == null) _urpShader = Shader.Find("Standard"); // Fallback
            }

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            // Create a new unique material to avoid affecting other objects
            Material mat = new Material(_urpShader);
            mat.color = color;
            
            // URP specific properties
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            renderer.sharedMaterial = mat;
        }

        public static Color GetRandomSkinTone()
        {
            Color[] tones = {
                new Color(0.95f, 0.75f, 0.65f),
                new Color(0.85f, 0.65f, 0.55f),
                new Color(0.75f, 0.55f, 0.45f)
            };
            return tones[Random.Range(0, tones.Length)];
        }

        public static System.Collections.IEnumerator FadeGroupCoroutine(CanvasGroup cg, float targetAlpha, float duration, bool blocks, bool interact)
        {
            if (cg == null) yield break;

            float startAlpha = cg.alpha;
            float elapsed = 0f;

            // Immediately update raycasting/interaction states
            cg.blocksRaycasts = blocks;
            cg.interactable   = interact;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // safe to use when Time.timeScale = 0 (paused)
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            cg.alpha = targetAlpha;
        }

        // --- Mock UI Helpers (Fixed Prefab Issue) ---

        public static Canvas CreateBaseCanvas(string name, int sortingOrder, Transform parent = null)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            go.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvas;
        }

        public static void SetupMockHUD(GPOyun.UI.HUDManager hud)
        {
            Canvas canvas = CreateBaseCanvas("MOCK_HUD", 100, hud.transform);
            
            // Viewfinder Group
            GameObject vf = new GameObject("Viewfinder");
            vf.transform.SetParent(canvas.transform, false);
            RectTransform vfRect = vf.AddComponent<RectTransform>();
            vfRect.sizeDelta = new Vector2(480, 320);
            vfRect.anchoredPosition = Vector2.zero;

            // Viewfinder frame backing and outline
            var vfImg = vf.AddComponent<UnityEngine.UI.Image>();
            vfImg.color = new Color(1f, 1f, 1f, 0.05f); // ultra-subtle backing tint
            var vfOutline = vf.AddComponent<UnityEngine.UI.Outline>();
            vfOutline.effectColor = new Color(1f, 1f, 1f, 0.6f);
            vfOutline.effectDistance = new Vector2(2f, 2f);

            // Wide-screen letterbox overlay - Top black bar
            GameObject topBar = new GameObject("VfTopBar");
            topBar.transform.SetParent(vf.transform, false);
            var topImg = topBar.AddComponent<UnityEngine.UI.Image>();
            topImg.color = new Color(0.04f, 0.04f, 0.06f, 0.85f);
            var topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(-4f, 1f);
            topRect.anchorMax = new Vector2(5f, 1f);
            topRect.anchoredPosition = new Vector2(0f, 160f);
            topRect.sizeDelta = new Vector2(0f, 400f);

            // Wide-screen letterbox overlay - Bottom black bar
            GameObject bottomBar = new GameObject("VfBottomBar");
            bottomBar.transform.SetParent(vf.transform, false);
            var bottomImg = bottomBar.AddComponent<UnityEngine.UI.Image>();
            bottomImg.color = new Color(0.04f, 0.04f, 0.06f, 0.85f);
            var bottomRect = bottomBar.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(-4f, 0f);
            bottomRect.anchorMax = new Vector2(5f, 0f);
            bottomRect.anchoredPosition = new Vector2(0f, -160f);
            bottomRect.sizeDelta = new Vector2(0f, 400f);

            // Center targeting crosshair
            GameObject crosshair = new GameObject("VfCrosshair");
            crosshair.transform.SetParent(vf.transform, false);
            var crossText = crosshair.AddComponent<Text>();
            crossText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            crossText.fontSize = 24;
            crossText.color = new Color(1f, 0.85f, 0.2f, 0.8f); // Golden yellow
            crossText.text = "[  +  ]";
            crossText.alignment = TextAnchor.MiddleCenter;
            var crossRect = crosshair.GetComponent<RectTransform>();
            crossRect.anchoredPosition = Vector2.zero;
            crossRect.sizeDelta = new Vector2(120f, 60f);
            // Photo Count (Sticky Right)
            GameObject txtGo = new GameObject("PhotoText");
            txtGo.transform.SetParent(canvas.transform, false);
            Text txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 32;
            txt.alignment = TextAnchor.UpperRight;
            txt.color = Color.white;
            RectTransform txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchoredPosition = new Vector2(-50, -50);
            txtRect.anchorMax = Vector2.one;
            txtRect.anchorMin = Vector2.one;

            // Clock (Sticky Left - Cozy)
            GameObject clockGo = new GameObject("ClockText");
            clockGo.transform.SetParent(canvas.transform, false);
            Text clockTxt = clockGo.AddComponent<Text>();
            clockTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            clockTxt.fontSize = 28;
            clockTxt.alignment = TextAnchor.UpperLeft;
            clockTxt.color = StuccoWhite;
            RectTransform clockRect = clockGo.GetComponent<RectTransform>();
            clockRect.anchoredPosition = new Vector2(50, -50);
            clockRect.anchorMax = new Vector2(0, 1);
            clockRect.anchorMin = new Vector2(0, 1);
            
            hud.Initialize(vf, txt, clockTxt);
            EnsureCanvasRenderers(hud.transform);
        }

        private static void CreateCorner(Transform parent, Vector2 pos)
        {
            GameObject go = new GameObject("Corner");
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(1, 1, 1, 0.5f);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);
            rt.anchoredPosition = pos;
        }

        public static void SetupMockSplash(GPOyun.UI.SplashController splash)
        {
            Canvas canvas = CreateBaseCanvas("MOCK_SPLASH", 999, splash.transform);
            CanvasGroup cg = canvas.gameObject.AddComponent<CanvasGroup>();
            splash.Initialize(cg, null);
        }

        public static void SetupMockSettings(GPOyun.UI.SettingsController settings)
        {
            Canvas canvas = CreateBaseCanvas("MOCK_SETTINGS", 1000, settings.transform); // Topmost
            canvas.gameObject.SetActive(true); // Always on but hidden by alpha
            
            CanvasGroup cg = canvas.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Background (Semi-Transparent)
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            var img = bg.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.85f);
            bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bg.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            bg.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // Title
            GameObject txtGo = new GameObject("SettingsTitle");
            txtGo.transform.SetParent(canvas.transform, false);
            Text txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 60;
            txt.text = "SETTINGS";
            txt.color = StuccoWhite;
            txt.alignment = TextAnchor.MiddleCenter;
            RectTransform titleRect = txtGo.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 200);
            titleRect.sizeDelta = new Vector2(400, 100);

            // Sensitivity Slider (Container)
            GameObject sensGo = CreateSlider(canvas.transform, "SensitivitySlider", new Vector2(0, 50), "LOOK SENSITIVITY");
            
            // Volume Slider (Container)
            GameObject volGo = CreateSlider(canvas.transform, "VolumeSlider", new Vector2(0, -50), "MASTER VOLUME");

            // Interactive Resume Button for quick clicking!
            GameObject resumeGo = new GameObject("ResumeButton");
            resumeGo.transform.SetParent(canvas.transform, false);
            var btnRect = resumeGo.AddComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -130);
            btnRect.sizeDelta = new Vector2(250, 45);
            var btnImg = resumeGo.AddComponent<Image>();
            btnImg.color = Terracotta;
            var btnOutline = resumeGo.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0, 0, 0, 0.2f);
            
            var btn = resumeGo.AddComponent<Button>();
            btn.onClick.AddListener(settings.ToggleSettings);

            GameObject btnTxtGo = new GameObject("Label");
            btnTxtGo.transform.SetParent(resumeGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 16;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.color = Color.white;
            btnTxt.text = "RESUME GAME";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            var txtRect = btnTxtGo.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;

            // Instructions
            GameObject hintGo = new GameObject("HintText");
            hintGo.transform.SetParent(canvas.transform, false);
            Text hint = hintGo.AddComponent<Text>();
            hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hint.fontSize = 20;
            hint.text = "Press [ESC] to Resume\nPress [TAB] to switch Scene\nPress [N] for News Desk";
            hint.color = Color.gray;
            hint.alignment = TextAnchor.MiddleCenter;
            RectTransform hintRect = hintGo.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -250);
            hintRect.sizeDelta = new Vector2(400, 100);

            settings.Initialize(cg);
            EnsureCanvasRenderers(settings.transform);
        }

        private static GameObject CreateSlider(Transform parent, string name, Vector2 pos, string label)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 40);

            // Label
            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            Text t = labelGo.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = label;
            t.fontSize = 18;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            RectTransform lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchoredPosition = new Vector2(-150, 0);
            lrt.sizeDelta = new Vector2(200, 30);

            // Slider Background
            GameObject sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(root.transform, false);
            Slider slider = sliderGo.AddComponent<Slider>();
            RectTransform srt = sliderGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(50, 0);
            srt.sizeDelta = new Vector2(200, 20);

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(1, 1, 1, 0.2f);
            bg.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 10);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.sizeDelta = new Vector2(200, 10);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = CobaltBlue;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.fillRect.anchorMin = Vector2.zero;
            slider.fillRect.anchorMax = new Vector2(0, 1);
            slider.fillRect.offsetMin = Vector2.zero;
            slider.fillRect.offsetMax = Vector2.zero;

            slider.value = 0.5f; // Default
            return root;
        }

        public static void ApplyPlayerVisuals(GameObject player)
        {
            Transform head = player.transform.Find("Head");
            if (head == null) return;

            CreateEye(head, new Vector3(0.15f, 0.1f, 0.35f));
            CreateEye(head, new Vector3(-0.15f, 0.1f, 0.35f));
        }

        private static void CreateEye(Transform parent, Vector3 localPos)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(parent);
            eye.transform.localPosition = localPos;
            eye.transform.localScale    = new Vector3(0.1f, 0.1f, 0.1f);
            ApplyAesthetic(eye, Color.black, 0.8f);
            var col = eye.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);
        }

        public static void SetupPhotoGallery(GPOyun.UI.PhotoGalleryUI gallery)
        {
            Canvas canvas = CreateBaseCanvas("GALLERY_CANVAS", 500, gallery.transform);
            CanvasGroup cg = canvas.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Dark full-screen background
            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvas.transform, false);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 42; titleTxt.color = StuccoWhite;
            titleTxt.text = "PHOTO GALLERY  [G]";
            titleTxt.alignment = TextAnchor.MiddleCenter;
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1); titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = new Vector2(0, -50); titleRect.sizeDelta = new Vector2(0, 70);

            // Zoom panel (left side)
            GameObject zoomGo = new GameObject("ZoomPanel");
            zoomGo.transform.SetParent(canvas.transform, false);
            var zoomRect = zoomGo.AddComponent<RectTransform>();
            zoomRect.anchorMin = new Vector2(0.02f, 0.25f); zoomRect.anchorMax = new Vector2(0.55f, 0.9f);
            zoomRect.offsetMin = zoomRect.offsetMax = Vector2.zero;
            var rawImg = zoomGo.AddComponent<RawImage>();
            rawImg.color = new Color(1, 1, 1, 0.95f);

            // Meta label (below zoom)
            GameObject metaGo = new GameObject("MetaLabel");
            metaGo.transform.SetParent(canvas.transform, false);
            var metaTxt = metaGo.AddComponent<Text>();
            metaTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            metaTxt.fontSize = 22; metaTxt.color = StuccoWhite;
            metaTxt.text = "No photo selected.";
            metaTxt.alignment = TextAnchor.UpperLeft;
            var metaRect = metaGo.GetComponent<RectTransform>();
            metaRect.anchorMin = new Vector2(0.02f, 0.08f); metaRect.anchorMax = new Vector2(0.55f, 0.24f);
            metaRect.offsetMin = metaRect.offsetMax = Vector2.zero;

            // Score label
            GameObject scoreGo = new GameObject("ScoreLabel");
            scoreGo.transform.SetParent(canvas.transform, false);
            var scoreTxt = scoreGo.AddComponent<Text>();
            scoreTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreTxt.fontSize = 26; scoreTxt.color = new Color(0.9f, 0.75f, 0.3f);
            scoreTxt.text = "Score: –";
            scoreTxt.alignment = TextAnchor.UpperLeft;
            var scoreRect = scoreGo.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.02f, 0.02f); scoreRect.anchorMax = new Vector2(0.55f, 0.09f);
            scoreRect.offsetMin = scoreRect.offsetMax = Vector2.zero;

            // Thumbnail scroll row (right side, moved up to 0.19f for button stacking)
            GameObject rowRoot = new GameObject("ThumbnailRow");
            rowRoot.transform.SetParent(canvas.transform, false);
            var rowRect = rowRoot.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.57f, 0.19f); rowRect.anchorMax = new Vector2(0.98f, 0.9f);
            rowRect.offsetMin = rowRect.offsetMax = Vector2.zero;
            var layout = rowRoot.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8; layout.childControlWidth = true; layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            var csf = rowRoot.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Close Button
            GameObject closeGo = new GameObject("CloseBtn");
            closeGo.transform.SetParent(canvas.transform, false);
            var closeBtnRect = closeGo.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.57f, 0.10f); closeBtnRect.anchorMax = new Vector2(0.98f, 0.17f);
            closeBtnRect.offsetMin = closeBtnRect.offsetMax = Vector2.zero;
            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.2f, 0.22f, 0.28f);
            var closeBtn = closeGo.AddComponent<Button>();
            var closeBtnColors = closeBtn.colors;
            closeBtnColors.highlightedColor = new Color(0.3f, 0.33f, 0.42f);
            closeBtn.colors = closeBtnColors;
            closeBtn.onClick.AddListener(gallery.Hide);

            GameObject closeTxtGo = new GameObject("Text");
            closeTxtGo.transform.SetParent(closeGo.transform, false);
            var closeTxt = closeTxtGo.AddComponent<Text>();
            closeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeTxt.fontSize = 24; closeTxt.color = Color.white;
            closeTxt.text = "CLOSE GALLERY";
            closeTxt.alignment = TextAnchor.MiddleCenter;
            closeTxtGo.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            closeTxtGo.GetComponent<RectTransform>().anchorMax = Vector2.one;
            closeTxtGo.GetComponent<RectTransform>().offsetMin = closeTxtGo.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // Publish Button
            GameObject btnGo = new GameObject("PublishBtn");
            btnGo.transform.SetParent(canvas.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.57f, 0.01f); btnRect.anchorMax = new Vector2(0.98f, 0.09f);
            btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = Terracotta;
            var btn = btnGo.AddComponent<Button>();
            var btnColors = btn.colors;
            btnColors.highlightedColor = new Color(1f, 0.5f, 0.3f);
            btn.colors = btnColors;

            GameObject btnTxtGo = new GameObject("Text");
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 28; btnTxt.color = Color.white;
            btnTxt.text = "PUBLISH EDITION";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxtGo.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            btnTxtGo.GetComponent<RectTransform>().anchorMax = Vector2.one;
            btnTxtGo.GetComponent<RectTransform>().offsetMin = btnTxtGo.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            btn.onClick.AddListener(gallery.OnPublishClicked);

            gallery.galleryGroup      = cg;
            gallery.thumbnailRow      = rowRect;
            gallery.zoomImage         = rawImg;
            gallery.metaLabel         = metaTxt;
            gallery.scoreLabel        = scoreTxt;
            gallery.publishButton     = btn;
            gallery.publishButtonText = btnTxt;

            EnsureCanvasRenderers(gallery.transform);
        }

        public static void EnsureCanvasRenderers(Transform t)
        {
            if (t.GetComponent<UnityEngine.UI.Graphic>() != null)
            {
                if (t.GetComponent<CanvasRenderer>() == null)
                {
                    t.gameObject.AddComponent<CanvasRenderer>();
                }
            }
            foreach (Transform child in t)
            {
                EnsureCanvasRenderers(child);
            }
        }
    }
}

