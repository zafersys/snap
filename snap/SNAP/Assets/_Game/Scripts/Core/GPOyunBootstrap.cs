using UnityEngine;
using GPOyun.Managers;
using GPOyun.Newspaper;
using GPOyun.NPC;
using GPOyun.Environment;
using GPOyun.UI;
using GPOyun.Player;
using GPOyun.CameraSystem;

namespace GPOyun.Core
{
    /// <summary>
    /// Entry point for the entire game scene.
    /// Spawns all managers, builds the town, creates the player and camera,
    /// and initialises the HUD / UI stack. 
    /// Invoked automatically by GPOyunEditorTools on first load OR by pressing Play.
    /// </summary>
    public class GPOyunBootstrap : MonoBehaviour
    {
        private void Start()
        {
            // If managers were already built in-editor via the menu tool,
            // they will already exist in the scene — don't build twice.
            if (FindAnyObjectByType<TimeManager>() == null)
            {
                NuclearColdStart();
            }
            else
            {
                Debug.Log("[Bootstrap] World already built (editor). Skipping cold start.");
                EnsureUIStackExists();
            }
        }

        private void EnsureUIStackExists()
        {
            // Ensure CameraController exists on the Main Camera so viewfinder/capture functions perfectly!
            var activeCam = Camera.main;
            if (activeCam == null)
            {
                var allCams = Object.FindObjectsByType<Camera>();
                if (allCams.Length > 0) activeCam = allCams[0];
            }
            if (activeCam != null && activeCam.GetComponent<CameraController>() == null)
            {
                activeCam.gameObject.AddComponent<CameraController>();
                Debug.Log("[Bootstrap] Self-healed: Added missing CameraController to Main Camera.");
            }

            GameObject uiRoot = GameObject.Find("[UI]");
            if (uiRoot == null)
            {
                uiRoot = new GameObject("[UI]");
                DontDestroyOnLoad(uiRoot);
            }

            // Ensure Splash
            if (Object.FindAnyObjectByType<SplashController>() == null)
            {
                var splashGo = new GameObject("SplashController");
                splashGo.transform.SetParent(uiRoot.transform);
                var splash = splashGo.AddComponent<SplashController>();
                VisualUtils.SetupMockSplash(splash);
            }

            // Ensure HUD
            var hud = Object.FindAnyObjectByType<HUDManager>();
            if (hud == null)
            {
                var hudGo = new GameObject("HUDManager");
                hudGo.transform.SetParent(uiRoot.transform);
                hud = hudGo.AddComponent<HUDManager>();
                VisualUtils.SetupMockHUD(hud);
            }
            else if (hud.viewfinderGroup == null)
            {
                VisualUtils.SetupMockHUD(hud);
            }

            // Ensure Settings
            var settings = Object.FindAnyObjectByType<SettingsController>();
            if (settings == null)
            {
                var settingsGo = new GameObject("SettingsController");
                settingsGo.transform.SetParent(uiRoot.transform);
                settings = settingsGo.AddComponent<SettingsController>();
                VisualUtils.SetupMockSettings(settings);
            }
            else if (settings.GetComponentInChildren<CanvasGroup>() == null)
            {
                VisualUtils.SetupMockSettings(settings);
            }

            // Ensure Editorial UI
            var editorial = Object.FindAnyObjectByType<EditorialUI>();
            if (editorial == null)
            {
                var editorialGo = new GameObject("EditorialUI");
                editorialGo.transform.SetParent(uiRoot.transform);
                editorial = editorialGo.AddComponent<EditorialUI>();
                SetupEditorialUI(editorial);
            }

            // Ensure Gallery UI
            var gallery = Object.FindAnyObjectByType<PhotoGalleryUI>();
            if (gallery == null)
            {
                var galleryGo = new GameObject("PhotoGalleryUI");
                galleryGo.transform.SetParent(uiRoot.transform);
                gallery = galleryGo.AddComponent<PhotoGalleryUI>();
                VisualUtils.SetupPhotoGallery(gallery);
            }
            else if (gallery.galleryGroup == null)
            {
                VisualUtils.SetupPhotoGallery(gallery);
            }

            // Ensure Journal UI
            var jUi = Object.FindAnyObjectByType<JournalUI>();
            if (jUi == null)
            {
                var jUiGo = new GameObject("JournalUI");
                jUiGo.transform.SetParent(uiRoot.transform);
                jUi = jUiGo.AddComponent<JournalUI>();
            }
            else if (jUi.journalCanvasGroup == null)
            {
                // Force Awake/Start re-initialization if canvas fields are blank
                jUi.SendMessage("SetupUI", SendMessageOptions.DontRequireReceiver);
            }

            // Ensure Newspaper Board UI
            var boardUi = Object.FindAnyObjectByType<NewspaperBoardUI>();
            if (boardUi == null)
            {
                var boardUiGo = new GameObject("NewspaperBoardUI");
                boardUiGo.transform.SetParent(uiRoot.transform);
                boardUi = boardUiGo.AddComponent<NewspaperBoardUI>();
            }
            else if (boardUi.boardCanvasGroup == null)
            {
                boardUi.SendMessage("SetupUI", SendMessageOptions.DontRequireReceiver);
            }

            // Ensure Photo Review UI
            var review = Object.FindAnyObjectByType<PhotoReviewUI>();
            if (review == null)
            {
                var reviewGo = new GameObject("PhotoReviewUI");
                reviewGo.transform.SetParent(uiRoot.transform);
                review = reviewGo.AddComponent<PhotoReviewUI>();
            }
            else if (review.GetComponentInChildren<CanvasGroup>() == null)
            {
                review.SendMessage("BuildUI", SendMessageOptions.DontRequireReceiver);
            }

            // Ensure Centralized Input Listener
            if (Object.FindAnyObjectByType<GPOyun.InputSystem.GlobalInputListener>() == null)
            {
                var inputGo = new GameObject("GlobalInputListener");
                inputGo.transform.SetParent(uiRoot.transform);
                inputGo.AddComponent<GPOyun.InputSystem.GlobalInputListener>();
            }

            // Ensure EventSystem
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.transform.SetParent(uiRoot.transform);
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Ensure 10 Refactored & Use Case singletons exist!
            if (Object.FindAnyObjectByType<SocialDatabase>() == null)
            {
                var go = new GameObject("SocialDatabase");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<SocialDatabase>();
            }
            if (Object.FindAnyObjectByType<CameraSystem.ViewfinderManager>() == null)
            {
                var go = new GameObject("ViewfinderManager");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<CameraSystem.ViewfinderManager>();
            }
            if (Object.FindAnyObjectByType<UI.GalleryController>() == null)
            {
                var go = new GameObject("GalleryController");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<UI.GalleryController>();
            }
            if (Object.FindAnyObjectByType<StoryComposer>() == null)
            {
                var go = new GameObject("StoryComposer");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<StoryComposer>();
            }
            if (Object.FindAnyObjectByType<SocialHistoryLogger>() == null)
            {
                var go = new GameObject("SocialHistoryLogger");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<SocialHistoryLogger>();
            }

            if (Object.FindAnyObjectByType<UseCases.TakePictureUseCase>() == null)
            {
                var go = new GameObject("TakePictureUseCase");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<UseCases.TakePictureUseCase>();
            }
            if (Object.FindAnyObjectByType<UseCases.SelectPictureUseCase>() == null)
            {
                var go = new GameObject("SelectPictureUseCase");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<UseCases.SelectPictureUseCase>();
            }
            if (Object.FindAnyObjectByType<UseCases.ComposeEditorialUseCase>() == null)
            {
                var go = new GameObject("ComposeEditorialUseCase");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<UseCases.ComposeEditorialUseCase>();
            }
            if (Object.FindAnyObjectByType<UseCases.SyncSocialDbUseCase>() == null)
            {
                var go = new GameObject("SyncSocialDbUseCase");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<UseCases.SyncSocialDbUseCase>();
            }
            if (Object.FindAnyObjectByType<UseCases.ApplyPublishingImpactUseCase>() == null)
            {
                var go = new GameObject("ApplyPublishingImpactUseCase");
                go.transform.SetParent(uiRoot.transform);
                go.AddComponent<UseCases.ApplyPublishingImpactUseCase>();
            }

            Debug.Log("[Bootstrap] UI Stack successfully verified and filled missing parts.");
        }

        public void NuclearColdStart()
        {
            Debug.Log("[Bootstrap] === NUCLEAR COLD START ===");

            // ── 0. CLEANUP SCENE DEFAULTS ──────────────────────────────────
            // The scene's baked-in "Main Camera" has an AudioListener.
            // Destroy it immediately (DestroyImmediate so it's gone THIS frame)
            // before we create our own camera to avoid the "2 AudioListeners" warning.
            var sceneCamera = GameObject.Find("Main Camera");
            if (sceneCamera != null && sceneCamera != this.gameObject)
            {
                DestroyImmediate(sceneCamera);
                Debug.Log("[Bootstrap] Removed scene's default Main Camera.");
            }
            // Also sweep any stray AudioListeners left from prior play sessions
            foreach (var al in FindObjectsByType<AudioListener>())
                DestroyImmediate(al.gameObject.name == "Main Camera" ? al.gameObject : (Object)al);

            // ── 1. MANAGERS ────────────────────────────────────────────────
            GameObject managersRoot = new GameObject("[MANAGERS]");

            // Time Manager
            var timeManagerGo = new GameObject("TimeManager");
            timeManagerGo.transform.SetParent(managersRoot.transform);
            timeManagerGo.AddComponent<TimeManager>();

            // NPC Manager (must exist before town spawns NPCs)
            var npcManagerGo = new GameObject("NPCManager");
            npcManagerGo.transform.SetParent(managersRoot.transform);
            npcManagerGo.AddComponent<NPCManager>();

            // Newspaper Manager
            var newspaperManagerGo = new GameObject("NewspaperManager");
            newspaperManagerGo.transform.SetParent(managersRoot.transform);
            newspaperManagerGo.AddComponent<NewspaperManager>();

            // Game Manager
            var gameManagerGo = new GameObject("GameManager");
            gameManagerGo.transform.SetParent(managersRoot.transform);
            gameManagerGo.AddComponent<GameManager>();

            // Relationship Matrix
            var relGo = new GameObject("RelationshipMatrix");
            relGo.transform.SetParent(managersRoot.transform);
            var matrix = relGo.AddComponent<RelationshipMatrix>();
            matrix.InitializeMatrix();

            // Journal Manager
            var journalGo = new GameObject("JournalManager");
            journalGo.transform.SetParent(managersRoot.transform);
            journalGo.AddComponent<JournalManager>();

            // Photo Scorer
            var scorerGo = new GameObject("PhotoScorer");
            scorerGo.transform.SetParent(managersRoot.transform);
            scorerGo.AddComponent<PhotoScorer>();

            Debug.Log("[Bootstrap] Managers created.");

            // ── 2b. LIGHTING + ATMOSPHERE ────────────────────────────
            var sunGo = new GameObject("Sun_Directional");
            sunGo.transform.SetParent(managersRoot.transform);
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0);
            var sunLight = sunGo.AddComponent<Light>();
            sunLight.type           = LightType.Directional;
            sunLight.intensity      = 1.2f;
            sunLight.color          = new Color(1f, 0.95f, 0.85f);
            sunLight.shadows        = LightShadows.Soft;
            sunLight.shadowStrength = 0.75f;
            RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.5f);

            var atmosphereGo = new GameObject("AtmosphereManager");
            atmosphereGo.transform.SetParent(managersRoot.transform);
            var atm = atmosphereGo.AddComponent<AtmosphereManager>();
            atm.Initialize(sunLight);

            Debug.Log("[Bootstrap] Sun and atmosphere created.");

            // ── 3. ENVIRONMENT ─────────────────────────────────────
            GameObject townRoot = new GameObject("[TOWN]");
            var builder = townRoot.AddComponent<TownSquareBuilder>();
            builder.Build();

            Debug.Log("[Bootstrap] Town built.");

            // ── 3. PLAYER ──────────────────────────────────────────────────
            // Capsule body
            GameObject playerGo = new GameObject("[PLAYER]");
            playerGo.transform.position = new Vector3(0, 1f, -8f);

            GameObject playerBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerBody.name = "Body";
            playerBody.transform.SetParent(playerGo.transform);
            playerBody.transform.localPosition = Vector3.zero;
            VisualUtils.ApplyAesthetic(playerBody, VisualUtils.CrimsonRed);

            GameObject playerHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            playerHead.name = "Head";
            playerHead.transform.SetParent(playerGo.transform);
            playerHead.transform.localPosition = new Vector3(0, 1.2f, 0);
            playerHead.transform.localScale = Vector3.one * 0.6f;
            VisualUtils.ApplyAesthetic(playerHead, VisualUtils.CrimsonRed);
            VisualUtils.ApplyPlayerVisuals(playerGo);

            // Remove colliders from head/body so they don't interfere with physics
            var headCol = playerHead.GetComponent<Collider>();
            if (headCol != null) Destroy(headCol);

            // Add a CharacterController for movement so the player doesn't clip through ground
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0, 1f, 0);

            playerGo.AddComponent<PlayerController>();

            Debug.Log("[Bootstrap] Player created.");

            // ── 4. CAMERA ──────────────────────────────────────────────────
            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(playerGo.transform);
            // Eye-level, slightly forward
            cameraGo.transform.localPosition = new Vector3(0, 1.6f, 0.2f);
            cameraGo.transform.localRotation = Quaternion.identity;

            var cam = cameraGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.1f;

            // Destroy ALL existing AudioListeners (scene default camera has one baked in)
            foreach (var al in FindObjectsByType<AudioListener>())
                Destroy(al);
            cameraGo.AddComponent<AudioListener>(); // Exactly one

            cameraGo.AddComponent<CameraController>();

            Debug.Log("[Bootstrap] Camera created.");

            // ── 5. UI STACK ────────────────────────────────────────────────
            GameObject uiRoot = new GameObject("[UI]");

            // Splash
            var splashGo = new GameObject("SplashController");
            splashGo.transform.SetParent(uiRoot.transform);
            var splash = splashGo.AddComponent<SplashController>();
            VisualUtils.SetupMockSplash(splash);

            // HUD
            var hudGo = new GameObject("HUDManager");
            hudGo.transform.SetParent(uiRoot.transform);
            var hud = hudGo.AddComponent<HUDManager>();
            VisualUtils.SetupMockHUD(hud);

            // Settings
            var settingsGo = new GameObject("SettingsController");
            settingsGo.transform.SetParent(uiRoot.transform);
            var settings = settingsGo.AddComponent<SettingsController>();
            VisualUtils.SetupMockSettings(settings);

            // Editorial Desk
            var editorialGo = new GameObject("EditorialUI");
            editorialGo.transform.SetParent(uiRoot.transform);
            var editorial = editorialGo.AddComponent<EditorialUI>();
            SetupEditorialUI(editorial);

            // Gallery UI
            var galleryGo = new GameObject("PhotoGalleryUI");
            galleryGo.transform.SetParent(uiRoot.transform);
            var gallery = galleryGo.AddComponent<PhotoGalleryUI>();
            VisualUtils.SetupPhotoGallery(gallery);

            // Journal UI
            var jUiGo = new GameObject("JournalUI");
            jUiGo.transform.SetParent(uiRoot.transform);
            jUiGo.AddComponent<JournalUI>();

            // Newspaper Board UI
            var boardUiGo = new GameObject("NewspaperBoardUI");
            boardUiGo.transform.SetParent(uiRoot.transform);
            boardUiGo.AddComponent<NewspaperBoardUI>();

            // Centralized high-reliability input listener
            var inputGo = new GameObject("GlobalInputListener");
            inputGo.transform.SetParent(uiRoot.transform);
            inputGo.AddComponent<GPOyun.InputSystem.GlobalInputListener>();

            // EventSystem for UI interactions (GraphicRaycaster support)
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(uiRoot.transform);
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            Debug.Log("[Bootstrap] UI stack and EventSystem created.");

            Debug.Log("[Bootstrap] === COLD START COMPLETE. Press Play and explore! ===");
        }

        private void SetupEditorialUI(EditorialUI editorial)
        {
            var canvas = VisualUtils.CreateBaseCanvas("MOCK_EDITORIAL", 200, editorial.transform);
            var cg = canvas.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Status text
            var txtGo = new GameObject("StatusText");
            txtGo.transform.SetParent(canvas.transform, false);
            var txt = txtGo.AddComponent<UnityEngine.UI.Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 36;
            txt.text = "EDITORIAL DESK";
            txt.color = VisualUtils.StuccoWhite;
            txt.alignment = TextAnchor.MiddleCenter;
            var rt = txtGo.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 100);
            rt.sizeDelta = new Vector2(500, 60);

            editorial.editorialCanvasGroup = cg;
            editorial.statusText = txt;

            VisualUtils.EnsureCanvasRenderers(editorial.transform);
        }
    }
}
