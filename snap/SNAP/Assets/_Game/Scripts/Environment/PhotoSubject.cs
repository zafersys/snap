using UnityEngine;

namespace GPOyun.Environment
{
    /// <summary>
    /// Attached to NPCs or Objects that the camera should recognize.
    /// Used to determine the 'Subject' of a photo.
    /// </summary>
    public class PhotoSubject : MonoBehaviour
    {
        [Header("Metadata")]
        public string SubjectName;
        public GPOyun.Newspaper.NewsCategory PrimaryCategory = GPOyun.Newspaper.NewsCategory.Local;
        
        [Range(0, 100)]
        public int InterestLevel = 50;

        public string GetDescription() => $"Subject: {SubjectName} ({PrimaryCategory})";
    }
}
