using System;
using System.IO;
using UnityEngine;
using GPOyun.Newspaper;
using GPOyun.CameraSystem;

namespace GPOyun.UseCases
{
    /// <summary>
    /// Encapsulates the Use Case of capturing and saving a viewport picture to disk.
    /// </summary>
    public class TakePictureUseCase : MonoBehaviour
    {
        public static TakePictureUseCase Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public PhotoData Execute(Camera activeCam)
        {
            if (activeCam == null)
            {
                Debug.LogWarning("[TakePictureUseCase] Active camera is missing!");
                return null;
            }

            Environment.PhotoSubject targetSubject = null;
            int score = 30; // base scenic score

            if (ViewfinderManager.Instance != null && ViewfinderManager.Instance.IsAimingAtSubject(activeCam, out Environment.PhotoSubject subject))
            {
                targetSubject = subject;
                score = ViewfinderManager.Instance.CalculateCompositionScore(activeCam, subject);
            }

            // Capture Render Texture to 2D texture
            int width = 512;
            int height = 512;
            RenderTexture rt = new RenderTexture(width, height, 24);
            activeCam.targetTexture = rt;
            activeCam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            activeCam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            // Create directories and write physical PNG on disk
            string dir = Path.Combine(Application.dataPath, "_Game/CapturedPhotos");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string uuid = Guid.NewGuid().ToString("N").Substring(0, 8);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string subjectName = targetSubject != null ? targetSubject.SubjectName : "Scenic";
            string filename = $"{uuid}_{timestamp}_{subjectName}.png";
            string fullPath = Path.Combine(dir, filename);

            try
            {
                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(fullPath, bytes);
                Debug.Log($"[TakePictureUseCase] Capture written to: {fullPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TakePictureUseCase] Failed to save PNG: {ex.Message}");
            }

            PhotoData data = new PhotoData
            {
                CapturedTexture = tex,
                WorldPosition = activeCam.transform.position,
                PrimarySubject = targetSubject,
                CompositionScore = score,
                FilePath = fullPath
            };

            // Register photo into the NewspaperManager pool
            if (NewspaperManager.Instance != null)
            {
                NewspaperManager.Instance.StorePhoto(data);
            }

            return data;
        }
    }
}
