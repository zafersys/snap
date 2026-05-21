using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GPOyun.CameraSystem;
using GPOyun.Environment;
using GPOyun.Newspaper;
using GPOyun.Managers;
using GPOyun.NPC;
using GPOyun.Core;

namespace GPOyun.Tests
{
    public class ActivityDiagramTests
    {
        private GameObject _cameraGo;
        private CameraController _cameraController;
        private GameObject _newspaperManagerGo;
        private NewspaperManager _newspaperManager;
        
        [SetUp]
        public void Setup()
        {
            // Setup NewspaperManager
            _newspaperManagerGo = new GameObject("NewspaperManager");
            _newspaperManager = _newspaperManagerGo.AddComponent<NewspaperManager>();

            // Setup Camera
            _cameraGo = new GameObject("MainCamera");
            _cameraGo.tag = "MainCamera";
            var cam = _cameraGo.AddComponent<UnityEngine.Camera>();
            _cameraController = _cameraGo.AddComponent<CameraController>();
            
            // Wait for Awake calls
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_cameraGo);
            Object.DestroyImmediate(_newspaperManagerGo);
            
            var npcs = Object.FindObjectsByType<NPCController>();
            foreach (var npc in npcs) Object.DestroyImmediate(npc.gameObject);
        }

        [UnityTest]
        public IEnumerator Camera_Raycast_HitsGround_StoresNullSubject()
        {
            // Arrange: Create a ground plane on Layer 6
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.layer = 6;
            ground.transform.position = new Vector3(0, -1, 5);
            ground.transform.rotation = Quaternion.Euler(-90, 0, 0); // Face camera
            
            _cameraGo.transform.position = Vector3.zero;
            _cameraGo.transform.rotation = Quaternion.identity;
            
            yield return null; // Wait a frame for physics to register

            // Act: Use Reflection to force TryCapture() since it's private and tied to Input
            var method = typeof(CameraController).GetMethod("TryCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_cameraController, null);

            // Assert
            var photos = _newspaperManager.GetTodaysPhotos();
            Assert.AreEqual(1, photos.Count, "Photo should be captured");
            Assert.IsNull(photos[0].PrimarySubject, "Subject should be null because Layer 6 is ignored in the Raycast layer mask.");

            Object.DestroyImmediate(ground);
        }

        [UnityTest]
        public IEnumerator Camera_Raycast_HitsSubject_StoresValidSubject()
        {
            // Arrange: Create a valid PhotoSubject
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.position = new Vector3(0, 0, 5); // Directly in front of camera
            var subject = target.AddComponent<PhotoSubject>();
            subject.SubjectName = "Test Subject";
            
            _cameraGo.transform.position = Vector3.zero;
            _cameraGo.transform.rotation = Quaternion.identity;

            yield return null; // Wait for physics

            // Act
            var method = typeof(CameraController).GetMethod("TryCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_cameraController, null);

            // Assert
            var photos = _newspaperManager.GetTodaysPhotos();
            Assert.AreEqual(1, photos.Count, "Photo should be captured");
            Assert.IsNotNull(photos[0].PrimarySubject, "Subject should not be null.");
            Assert.AreEqual("Test Subject", photos[0].PrimarySubject.SubjectName, "Subject name should match the captured target.");

            Object.DestroyImmediate(target);
        }

        [UnityTest]
        public IEnumerator Camera_Cooldown_PreventsRapidCapture()
        {
            // Act: Fire twice immediately
            var method = typeof(CameraController).GetMethod("TryCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_cameraController, null);
            method.Invoke(_cameraController, null);

            // Assert
            var photos = _newspaperManager.GetTodaysPhotos();
            Assert.AreEqual(1, photos.Count, "Only one photo should be captured due to cooldown.");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator NPCManager_CorrectlyRegistersAndMaintainsNPCs()
        {
            // Arrange
            var npcManagerGo = new GameObject("NPCManager");
            var npcManager = npcManagerGo.AddComponent<NPCManager>();

            var npc1Go = new GameObject("NPC1");
            var npc1 = npc1Go.AddComponent<NPCController>();
            
            yield return null; // Wait for Start() where registration happens

            // Assert
            Assert.AreEqual(1, npcManager.GetAll().Count, "NPC should have self-registered on Start.");
            Assert.AreEqual(npc1, npcManager.GetAll()[0]);

            // Act: Destroy NPC
            Object.DestroyImmediate(npc1Go);
            yield return null;

            // Assert
            Assert.AreEqual(0, npcManager.GetAll().Count, "NPC should have unregistered on Destroy.");
            
            Object.DestroyImmediate(npcManagerGo);
        }
    }
}
