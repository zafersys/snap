using UnityEngine;
using GPOyun.Core;
using GPOyun.Newspaper;

namespace GPOyun.UseCases
{
    /// <summary>
    /// Encapsulates the Use Case of shifting relationship scores, emoting character reactions, and logging consequences after newspaper distribution.
    /// </summary>
    public class ApplyPublishingImpactUseCase : MonoBehaviour
    {
        public static ApplyPublishingImpactUseCase Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Execute(NewsStory story)
        {
            if (story == null) return;

            // Trigger legacy relationship shifts
            if (RelationshipMatrix.Instance != null)
            {
                RelationshipMatrix.Instance.ProcessPublishingEvent(story);
            }

            // Sync visual highlights in the Observational Feed using pure emojis
            if (SocialHistoryLogger.Instance != null && story.SourcePhoto != null && story.SourcePhoto.PrimarySubject != null)
            {
                string subjectName = story.SourcePhoto.PrimarySubject.SubjectName;
                string emojiAction = (story.Category == NewsCategory.Scandal) ? "💔 /!\\" : "👑 ❤️";
                string chain = $"📰 👤 {subjectName} ➡️ {emojiAction}";
                SocialHistoryLogger.Instance.LogObservation(chain);
            }

            // Trigger NPC pride/shock emotes and updates
            NPC.NPCController[] npcs = Object.FindObjectsByType<NPC.NPCController>();
            if (npcs != null && story.SourcePhoto != null && story.SourcePhoto.PrimarySubject != null)
            {
                string subjectName = story.SourcePhoto.PrimarySubject.SubjectName;
                foreach (var npc in npcs)
                {
                    if (npc.NpcName == subjectName)
                    {
                        // Direct impact on the photographed subject!
                        if (story.Category == NewsCategory.Scandal)
                        {
                            npc.currentEmotion = NPC.EmotionType.Angry;
                        }
                        else
                        {
                            npc.currentEmotion = NPC.EmotionType.Happy;
                        }
                    }
                }
            }

            // Persist the fresh post-publishing snapshot to disk
            if (SyncSocialDbUseCase.Instance != null)
            {
                SyncSocialDbUseCase.Instance.Execute();
            }
        }
    }
}
