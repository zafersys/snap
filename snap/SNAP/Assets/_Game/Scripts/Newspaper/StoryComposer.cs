using UnityEngine;

namespace GPOyun.Newspaper
{
    /// <summary>
    /// Decoupled component that automates headline narrative composition based on composition scores and subject parameters.
    /// </summary>
    public class StoryComposer : MonoBehaviour
    {
        public static StoryComposer Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public string CreateHeadlineFor(PhotoData photo)
        {
            if (photo == null) return "A Quiet, Peaceful Day";
            if (photo.PrimarySubject == null) return "Beautiful Scenic Wilderness Captured!";

            string name = photo.PrimarySubject.SubjectName;
            int score = photo.CompositionScore;

            if (photo.PrimarySubject.PrimaryCategory == NewsCategory.Scandal)
            {
                if (score > 80) return $"SHOCKING: {name} Caught Red-Handed!";
                if (score > 50) return $"Rumors Swirl: What is {name} Hiding?";
                return $"Local Mystery Involving {name}";
            }
            else if (photo.PrimarySubject.PrimaryCategory == NewsCategory.Celebration)
            {
                if (score > 80) return $"HEROIC: {name} Brings Joy to Everyone!";
                return $"{name} Celebrates A Cozy Landmark!";
            }

            // Fallback templates based on composition score
            if (score > 80) return $"Stunning Spotlight on {name}!";
            if (score > 50) return $"A Scenic Glimpse of {name} Walking Around";
            return $"Spotted in Town: {name}";
        }

        public NewsCategory AssessCategory(PhotoData photo)
        {
            if (photo == null || photo.PrimarySubject == null) return NewsCategory.Local;
            return photo.PrimarySubject.PrimaryCategory;
        }
    }
}
