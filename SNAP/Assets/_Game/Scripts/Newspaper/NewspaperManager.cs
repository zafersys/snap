using System.Collections.Generic;
using UnityEngine;
using GPOyun.Managers;
using GPOyun.NPC;

namespace GPOyun.Newspaper
{
    // A1 Level Data Classes
    public class PhotoData
    {
        public Texture2D CapturedTexture;
        public Vector3   WorldPosition;
        public GPOyun.Environment.PhotoSubject PrimarySubject;
        public int       CompositionScore; // 0-100, drives headline selection
        public string    FilePath;         // Physical path on disk (uuid-date-subject.png)
    }

    public enum NewsCategory { Local, Global, Scandal, Celebration, Disaster }

    [System.Serializable]
    public class NewsStory
    {
        public string Headline;
        public Texture2D Photo;
        public NewsCategory Category;
        public PhotoData SourcePhoto; // Original captured metadata (NPCs, composition)
    }

    public class NewsPublishedData
    {
        public NewsStory FrontPage;
        public NewsStory SecondStory;
        public NewsStory SmallStory;
        public int DayIndex;
    }

    /// <summary>
    /// A1 Level Newspaper Manager
    /// Uses direct method calls instead of an EventBus.
    /// </summary>
    public class NewspaperManager : MonoBehaviour
    {
        public static NewspaperManager Instance { get; private set; }

        private readonly List<PhotoData> _capturedPhotos = new();
        private readonly List<NewsPublishedData> _history = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Direct method called by PlayerController
        public void StorePhoto(PhotoData photo)
        {
            _capturedPhotos.Add(photo);
            Debug.Log($"[NewspaperManager] Photo stored. Total today: {_capturedPhotos.Count}");
        }

        // Direct method called by TimeManager when Morning starts
        public void OnMorningArrived()
        {
            Debug.Log("[NewspaperManager] Morning arrived! Preparing newspaper roll for tomorrow.");
            _capturedPhotos.Clear();
        }

        public void PublishEdition(NewsStory front, NewsStory second = null, NewsStory small = null)
        {
            var newsData = new NewsPublishedData
            {
                FrontPage   = front,
                SecondStory = second,
                SmallStory  = small,
                DayIndex    = _history.Count + 1
            };

            _history.Add(newsData);
            Debug.Log($"[NewspaperManager] Published Edition #{newsData.DayIndex}!");

            if (GPOyun.Core.RelationshipMatrix.Instance != null)
            {
                GPOyun.Core.RelationshipMatrix.Instance.ProcessPublishingEvent(front);
            }
            
            // "Having a Conversation" - direct communication to all NPCs
            if (NPCManager.Instance != null)
            {
                foreach (var npc in NPCManager.Instance.GetAll())
                {
                    npc.ReceiveNews(newsData);
                }
            }
        }

        private void Start()
        {
            LoadSavedPhotosFromDisk();
        }

        private void LoadSavedPhotosFromDisk()
        {
            try
            {
                string folderPath = System.IO.Path.Combine(Application.dataPath, "_Game", "CapturedPhotos");
                if (!System.IO.Directory.Exists(folderPath)) return;

                string[] pngFiles = System.IO.Directory.GetFiles(folderPath, "*.png");
                foreach (string file in pngFiles)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(file);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        string filename = System.IO.Path.GetFileNameWithoutExtension(file);
                        string[] parts = filename.Split('_');
                        string subjectName = "Environmental";
                        if (parts.Length >= 3)
                        {
                            subjectName = parts[parts.Length - 1];
                        }

                        // Try to find the actual subject in the scene
                        GPOyun.Environment.PhotoSubject matchedSubject = null;
                        var allSubjects = Object.FindObjectsByType<GPOyun.Environment.PhotoSubject>();
                        foreach (var s in allSubjects)
                        {
                            if (s.SubjectName.Equals(subjectName, System.StringComparison.OrdinalIgnoreCase))
                            {
                                matchedSubject = s;
                                break;
                            }
                        }

                        PhotoData photo = new PhotoData
                        {
                            CapturedTexture = tex,
                            FilePath = "Assets/_Game/CapturedPhotos/" + System.IO.Path.GetFileName(file),
                            PrimarySubject = matchedSubject,
                            CompositionScore = matchedSubject != null ? matchedSubject.InterestLevel : Random.Range(10, 30),
                            WorldPosition = Vector3.zero
                        };

                        _capturedPhotos.Add(photo);
                        Debug.Log($"[NewspaperManager] Automatically loaded existing photo from disk: {photo.FilePath} (Subject: {subjectName})");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[NewspaperManager] LoadSavedPhotosFromDisk failed: {ex.Message}");
            }
        }

        public List<NewsPublishedData> GetHistory() => _history;
        public List<PhotoData> GetTodaysPhotos() => _capturedPhotos;
    }
}
