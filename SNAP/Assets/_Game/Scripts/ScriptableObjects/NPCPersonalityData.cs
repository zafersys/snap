using System;
using System.Collections.Generic;
using UnityEngine;
using GPOyun.Newspaper;
using GPOyun.NPC;

namespace GPOyun
{
    /// <summary>
    /// ScriptableObject that defines an NPC's personality and how they react
    /// to different news categories. Configure in the Unity Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "NPC_Personality", menuName = "GPOyun/NPC Personality")]
    public class NPCPersonalityData : ScriptableObject
    {
        [Header("Personality Traits")]
        [Range(0f, 1f)] public float Agreeableness   = 0.5f;
        [Range(0f, 1f)] public float Neuroticism      = 0.5f;
        [Range(0f, 1f)] public float Conscientiousness = 0.5f;
        [Range(0f, 1f)] public float Extraversion     = 0.5f;
        [Range(0f, 1f)] public float Openness         = 0.5f;

        [Header("News Reactions")]
        [SerializeField] private List<NewsReactionRule> reactionRules;

        public NewsReaction GetReactionTo(NewsCategory category)
        {
            foreach (var rule in reactionRules)
                if (rule.Category == category) return rule.Reaction;

            // Default fallback: neutral
            return new NewsReaction { Emotion = EmotionType.Neutral, Intensity = 0f };
        }
    }

    [Serializable]
    public class NewsReactionRule
    {
        public NewsCategory Category;
        public NewsReaction Reaction;
    }

    [Serializable]
    public class NewsReaction
    {
        public EmotionType Emotion;
        [Range(0f, 1f)] public float Intensity;
    }
}
