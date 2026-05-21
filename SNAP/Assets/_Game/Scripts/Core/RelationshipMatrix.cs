using System.Collections.Generic;
using UnityEngine;
using GPOyun.Newspaper;
using GPOyun.NPC;

namespace GPOyun.Core
{
    /// <summary>
    /// Tracks dynamic connections (friendship/rivalry) between each pair of NPC IDs.
    /// Symmetric and bidirectional. Clamped between -100 (Rivals) and +100 (Best Friends).
    /// </summary>
    public class RelationshipMatrix : MonoBehaviour
    {
        public static RelationshipMatrix Instance { get; private set; }

        private readonly Dictionary<string, int> _relationsTable = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void InitializeMatrix()
        {
            _relationsTable.Clear();
            Debug.Log("[RelationshipMatrix] Initialized empty social network.");
        }

        /// <summary>Formats a symmetric key like 'npc_0_npc_1' ensuring idA < idB.</summary>
        private string FormatKey(int idA, int idB)
        {
            int low = Mathf.Min(idA, idB);
            int high = Mathf.Max(idA, idB);
            return $"npc_{low}_npc_{high}";
        }

        public int GetRelationship(int idA, int idB)
        {
            if (idA == idB) return 100; // Self-relationship is perfect
            string key = FormatKey(idA, idB);
            return _relationsTable.TryGetValue(key, out int score) ? score : 0;
        }

        public void ModifyRelationship(int idA, int idB, int delta)
        {
            if (idA == idB) return;
            string key = FormatKey(idA, idB);
            int current = GetRelationship(idA, idB);
            int updated = Mathf.Clamp(current + delta, -100, 100);
            _relationsTable[key] = updated;

            Debug.Log($"[Social Network] Relationship {key} shifted: {current} -> {updated} (delta: {delta})");

            // Log observations for major relationship status shifts
            LogThresholdShift(idA, idB, current, updated);
        }

        private void LogThresholdShift(int idA, int idB, int oldScore, int newScore)
        {
            if (JournalManager.Instance == null) return;

            // Reduce threshold logging frequency so the journal is neat and clean!
            if (UnityEngine.Random.value > 0.35f) return;

            // Resolve character names from scene npcs
            string nameA = $"NPC_{idA}";
            string nameB = $"NPC_{idB}";
            var npcs = FindObjectsByType<NPC.NPCController>();
            foreach (var npc in npcs)
            {
                if (npc.NpcId == idA) nameA = npc.NpcName;
                if (npc.NpcId == idB) nameB = npc.NpcName;
            }

            // Friends -> Best Friends threshold
            if (oldScore < 86 && newScore >= 86)
            {
                JournalManager.Instance.AddObservation(
                    $"[{nameA}] [BEST-FRIENDS] <3 [{nameB}]",
                    new Color(0.9f, 0.75f, 0.3f) // Golden color
                );
            }
            // Strangers -> Friends threshold
            else if (oldScore < 50 && newScore >= 50)
            {
                JournalManager.Instance.AddObservation(
                    $"[{nameA}] [FRIENDS] :) [{nameB}]",
                    VisualUtils.CobaltBlue
                );
            }
            // Strangers -> Rivals threshold
            else if (oldScore > -50 && newScore <= -50)
            {
                JournalManager.Instance.AddObservation(
                    $"[{nameA}] [RIVALS] >:( [{nameB}]",
                    VisualUtils.Terracotta
                );
            }
            // Rivals -> Strangers (Peace)
            else if (oldScore <= -50 && newScore > -50)
            {
                JournalManager.Instance.AddObservation(
                    $"[{nameA}] [PEACE] [{nameB}]",
                    VisualUtils.StuccoWhite
                );
            }
        }

        /// <summary>
        /// Direct reaction to newspaper publishing. Shakes up the social dynamics of the town
        /// by shifting relationships based on individual NPC personality reactions.
        /// </summary>
        public void ProcessPublishingEvent(NewsStory story)
        {
            if (story == null) return;

            var npcs = FindObjectsByType<NPC.NPCController>();
            if (npcs.Length < 2) return;

            // Find NPC that represents this subject (if matching by name/id)
            NPC.NPCController subjectNpc = null;
            if (story.SourcePhoto != null && story.SourcePhoto.PrimarySubject != null)
            {
                string subjectName = story.SourcePhoto.PrimarySubject.SubjectName;
                foreach (var npc in npcs)
                {
                    if (npc.NpcName == subjectName || npc.gameObject.name == subjectName)
                    {
                        subjectNpc = npc;
                        break;
                    }
                }
            }

            // Fallback matching using headline contains character name
            if (subjectNpc == null)
            {
                foreach (var npc in npcs)
                {
                    if (!string.IsNullOrEmpty(npc.NpcName) && story.Headline.Contains(npc.NpcName))
                    {
                        subjectNpc = npc;
                        break;
                    }
                }
            }

            // Ultimate fallback: pick a random NPC as primary subject if name doesn't match directly
            if (subjectNpc == null)
            {
                subjectNpc = npcs[Random.Range(0, npcs.Length)];
            }

            string newsEmojis = (story.Category == NewsCategory.Scandal || story.Category == NewsCategory.Disaster)
                ? $"[NEWS-SCANDAL] -> [{subjectNpc.NpcName}] </3 /!\\"
                : $"[NEWS-HERO] -> [{subjectNpc.NpcName}] [*] <3";

            JournalManager.Instance?.AddObservation(
                newsEmojis,
                new Color(0.9f, 0.6f, 0.4f)
            );

            // Shakes up social dynamics: each NPC reacts based on their personality and news reaction rules
            foreach (var other in npcs)
            {
                if (other == subjectNpc) continue;

                int delta = 0;
                EmotionType reactionEmotion = EmotionType.Neutral;

                if (other.personality != null)
                {
                    var reaction = other.personality.GetReactionTo(story.Category);
                    reactionEmotion = reaction.Emotion;

                    switch (reaction.Emotion)
                    {
                        case EmotionType.Happy:
                            // Agreeableness boosts positive feelings towards subjects
                            delta = Mathf.RoundToInt(25f * reaction.Intensity * (1f + other.personality.Agreeableness));
                            break;

                        case NPC.EmotionType.Surprised:
                            // Neutral-to-positive shock
                            delta = Mathf.RoundToInt(15f * reaction.Intensity);
                            break;

                        case NPC.EmotionType.Angry:
                        case NPC.EmotionType.Disgusted:
                            // Low agreeableness makes characters much more hostile/envious
                            delta = -Mathf.RoundToInt(30f * reaction.Intensity * (2f - other.personality.Agreeableness));
                            break;

                        case NPC.EmotionType.Sad:
                            // Sad news creates shared sympathy or mild distance
                            delta = -Mathf.RoundToInt(15f * reaction.Intensity);
                            break;

                        case NPC.EmotionType.Fearful:
                            // Frightening stories increase tension and drops relationships slightly
                            delta = -Mathf.RoundToInt(20f * reaction.Intensity);
                            break;

                        default:
                            // Strangers gossip: minor random shift
                            delta = Random.Range(-5, 6);
                            break;
                    }
                }
                else
                {
                    // Fallback random
                    delta = Random.Range(-10, 11);
                }

                // Apply shifts
                ModifyRelationship(subjectNpc.NpcId, other.NpcId, delta);

                // Dynamic Player-NPC relationship adjustment:
                // Positive news outcomes raise their opinion of you (the journalist), negative drops it
                if (delta > 0)
                    other.relationshipWithPlayer = Mathf.Clamp(other.relationshipWithPlayer + 8, -100, 100);
                else if (delta < 0)
                    other.relationshipWithPlayer = Mathf.Clamp(other.relationshipWithPlayer - 8, -100, 100);

                // Set emotional visual cue directly on the NPC!
                other.currentEmotion = reactionEmotion;

                // Trigger dynamic floating emoji reactions on Santorini NPCs!
                string emoji = "💬";
                Color emojiColor = Color.white;

                if (delta >= 20)
                {
                    emoji = "❤️";
                    emojiColor = new Color(1f, 0.4f, 0.6f); // Soft Heart Pink
                }
                else if (delta > 0)
                {
                    emoji = "😊";
                    emojiColor = new Color(1f, 0.85f, 0.2f); // Golden Yellow
                }
                else if (delta <= -20)
                {
                    emoji = "💔";
                    emojiColor = new Color(0.85f, 0.35f, 0.2f); // Terracotta Red
                }
                else if (delta < 0)
                {
                    emoji = "😢";
                    emojiColor = new Color(0.2f, 0.6f, 0.9f); // Cobalt Blue
                }

                other.TriggerReaction(emoji, emojiColor);
            }

            // Primary subject spawns shiny star overlay and heavily shifts relationship with you!
            if (subjectNpc != null)
            {
                if (story.Category == NewsCategory.Scandal || story.Category == NewsCategory.Disaster)
                {
                    subjectNpc.relationshipWithPlayer = Mathf.Clamp(subjectNpc.relationshipWithPlayer - 40, -100, 100);
                }
                else
                {
                    subjectNpc.relationshipWithPlayer = Mathf.Clamp(subjectNpc.relationshipWithPlayer + 30, -100, 100);
                }
                subjectNpc.TriggerReaction("🌟", new Color(1f, 0.82f, 0.22f)); // Stars!
            }

            BroadcastRelationshipUpdate();
        }

        public void BroadcastRelationshipUpdate()
        {
            var npcs = FindObjectsByType<NPC.NPCController>();
            foreach (var npc in npcs)
            {
                npc.OnRelationshipUpdate();
            }
        }
    }
}
