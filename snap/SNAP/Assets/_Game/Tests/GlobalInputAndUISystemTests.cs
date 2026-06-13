using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GPOyun.Core;
using GPOyun.UI;
using GPOyun.InputSystem;

namespace GPOyun.Tests
{
    public class GlobalInputAndUISystemTests
    {
        private GameObject _uiRoot;
        private GameManager _gameManager;
        private GlobalInputListener _inputListener;
        private NewspaperBoardUI _boardUI;
        private PhotoGalleryUI _galleryUI;
        private JournalUI _journalUI;
        private SettingsController _settingsUI;

        [SetUp]
        public void Setup()
        {
            // Create GameManager instance
            var gmGo = new GameObject("GameManager");
            _gameManager = gmGo.AddComponent<GameManager>();

            // Setup UI Root Stack
            _uiRoot = new GameObject("[UI]");

            // Add UI Components
            var boardGo = new GameObject("NewspaperBoardUI");
            boardGo.transform.SetParent(_uiRoot.transform);
            _boardUI = boardGo.AddComponent<NewspaperBoardUI>();

            var galleryGo = new GameObject("PhotoGalleryUI");
            galleryGo.transform.SetParent(_uiRoot.transform);
            _galleryUI = galleryGo.AddComponent<PhotoGalleryUI>();
            VisualUtils.SetupPhotoGallery(_galleryUI);

            var journalGo = new GameObject("JournalUI");
            journalGo.transform.SetParent(_uiRoot.transform);
            _journalUI = journalGo.AddComponent<JournalUI>();

            var settingsGo = new GameObject("SettingsController");
            settingsGo.transform.SetParent(_uiRoot.transform);
            _settingsUI = settingsGo.AddComponent<SettingsController>();

            var cg = settingsGo.AddComponent<CanvasGroup>();
            _settingsUI.Initialize(cg);

            // Add Input Listener
            var inputGo = new GameObject("GlobalInputListener");
            inputGo.transform.SetParent(_uiRoot.transform);
            _inputListener = inputGo.AddComponent<GlobalInputListener>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(GameObject.Find("GameManager"));
            Object.DestroyImmediate(_uiRoot);
        }

        [UnityTest]
        public IEnumerator UIStack_Initializes_WithoutExceptions()
        {
            // Verify all objects and canvas groups initialized without crashing
            Assert.IsNotNull(_boardUI, "NewspaperBoardUI should be instantiated");
            Assert.IsNotNull(_galleryUI, "PhotoGalleryUI should be instantiated");
            Assert.IsNotNull(_journalUI, "JournalUI should be instantiated");
            Assert.IsNotNull(_settingsUI, "SettingsController should be instantiated");
            Assert.IsNotNull(_inputListener, "GlobalInputListener should be instantiated");

            Assert.IsNotNull(_boardUI.boardCanvasGroup, "NewspaperBoardUI CanvasGroup should be initialized");
            Assert.IsNotNull(_galleryUI.galleryGroup, "PhotoGalleryUI CanvasGroup should be initialized");

            yield return null;
        }

        [UnityTest]
        public IEnumerator NewspaperBoardUI_Toggle_PausesAndResumesGame()
        {
            bool isBatchMode = Application.isBatchMode;

            // Act: Show the board overlay
            _boardUI.Show();
            yield return null;

            // Assert: Game should be Paused, cursor unlocked
            Assert.AreEqual(GameManager.GameState.Paused, GameManager.Instance.CurrentState, "Game should be paused when board is active");
            if (!isBatchMode)
            {
                Assert.AreEqual(CursorLockMode.None, Cursor.lockState, "Cursor should be unlocked when board is active");
                Assert.IsTrue(Cursor.visible, "Cursor should be visible when board is active");
            }
            Assert.AreEqual(1f, _boardUI.boardCanvasGroup.alpha, "CanvasGroup alpha should be 1 when board is shown");

            // Act: Hide the board overlay
            _boardUI.Hide();
            yield return null;

            // Assert: Game should be Playing, cursor locked
            Assert.AreEqual(GameManager.GameState.Playing, GameManager.Instance.CurrentState, "Game should resume playing when board is closed");
            if (!isBatchMode)
            {
                Assert.AreEqual(CursorLockMode.Locked, Cursor.lockState, "Cursor should be locked when board is closed");
                Assert.IsFalse(Cursor.visible, "Cursor should be hidden when board is closed");
            }
            Assert.AreEqual(0f, _boardUI.boardCanvasGroup.alpha, "CanvasGroup alpha should be 0 when board is closed");
        }

        [UnityTest]
        public IEnumerator UIStack_AdheresTo_MutualExclusionRules()
        {
            // Act: Open the Gallery first
            _galleryUI.Show();
            yield return null;

            Assert.AreEqual(1f, _galleryUI.galleryGroup.alpha, "Gallery should be visible");
            Assert.AreEqual(0f, _boardUI.boardCanvasGroup.alpha, "Newspaper Board should be closed");

            // Act: Open the Newspaper Board
            _boardUI.Show();
            yield return null;

            // Assert: Gallery should close, Newspaper Board should open
            Assert.AreEqual(0f, _galleryUI.galleryGroup.alpha, "Gallery should have closed when Newspaper Board opened");
            Assert.AreEqual(1f, _boardUI.boardCanvasGroup.alpha, "Newspaper Board should be open");

            // Act: Open Settings
            // Settings uses internal method trigger via reflection/ToggleSettings
            _settingsUI.ToggleSettings();
            yield return null;

            // Assert: Newspaper Board should close
            Assert.AreEqual(0f, _boardUI.boardCanvasGroup.alpha, "Newspaper Board should close when Settings opens");
        }
    }
}
