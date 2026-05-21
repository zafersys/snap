using UnityEngine;
using GPOyun.Newspaper;

namespace GPOyun.UseCases
{
    /// <summary>
    /// Encapsulates the Use Case of preparing and publishing an editorial front page story.
    /// </summary>
    public class ComposeEditorialUseCase : MonoBehaviour
    {
        public static ComposeEditorialUseCase Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public NewsStory Execute(PhotoData photo)
        {
            if (photo == null)
            {
                Debug.LogWarning("[ComposeEditorialUseCase] Cannot compose an editorial story with no selection!");
                return null;
            }

            string headline = "A Quiet Day In Town";
            NewsCategory category = NewsCategory.Local;

            if (StoryComposer.Instance != null)
            {
                headline = StoryComposer.Instance.CreateHeadlineFor(photo);
                category = StoryComposer.Instance.AssessCategory(photo);
            }

            NewsStory story = new NewsStory
            {
                Headline = headline,
                Photo = photo.CapturedTexture,
                Category = category,
                SourcePhoto = photo
            };

            // Deliver edition to the physical newspaper board system
            if (NewspaperManager.Instance != null)
            {
                NewspaperManager.Instance.PublishEdition(story);
            }

            // Sync visual consequence triggers (e.g. NPC reaction loops and db sync)
            if (ApplyPublishingImpactUseCase.Instance != null)
            {
                ApplyPublishingImpactUseCase.Instance.Execute(story);
            }

            return story;
        }
    }
}
