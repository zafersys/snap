using UnityEngine;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.Appraisal
{
    public struct AppraisalResult
    {
        public EmotionType NewEmotion;
        public float ArousalDelta;
        public int RelationshipDelta;
    }

    public class NPCAppraisalEngine : MonoBehaviour
    {
        private NPCPersonalityData _personality;
        private Memory.NPCMemoryStream _memoryStream;

        public void Initialize(NPCPersonalityData personality, Memory.NPCMemoryStream memoryStream = null)
        {
            _personality = personality;
            _memoryStream = memoryStream;
        }

        public AppraisalResult Evaluate(Stimulus stimulus, EmotionType currentEmotion)
        {
            AppraisalResult result = new AppraisalResult
            {
                NewEmotion = currentEmotion,
                ArousalDelta = 0f,
                RelationshipDelta = 0
            };

            if (_personality == null) return result;

            switch (stimulus.Type)
            {
                case StimulusType.CameraShutter:
                case StimulusType.CameraFlash:
                case StimulusType.PlayerAimedCamera:
                    // 1. Check Instincts First! (Overrides personality)
                    if (_memoryStream != null && _memoryStream.HasInstinct("CameraShy"))
                    {
                        result.NewEmotion = EmotionType.Fearful;
                        result.ArousalDelta = 1.0f;
                        result.RelationshipDelta = -30;
                        break;
                    }

                    // 2. Evaluate based on Neuroticism and Agreeableness vs Extraversion
                    if (_personality.Neuroticism > 0.6f || _personality.Agreeableness < 0.4f)
                    {
                        result.NewEmotion = EmotionType.Angry;
                        result.ArousalDelta = 0.5f * stimulus.Intensity;
                        result.RelationshipDelta = -15;
                    }
                    else if (_personality.Extraversion > 0.7f)
                    {
                        result.NewEmotion = EmotionType.Happy;
                        result.ArousalDelta = 0.3f * stimulus.Intensity;
                        result.RelationshipDelta = +10;
                    }
                    else
                    {
                        result.NewEmotion = EmotionType.Neutral;
                        result.RelationshipDelta = -2; // Minor annoyance for shy people
                    }
                    break;
                case StimulusType.FriendlyGreeting:
                    result.NewEmotion = EmotionType.Happy;
                    result.RelationshipDelta = +5;
                    break;
                case StimulusType.HostileConfrontation:
                    result.NewEmotion = EmotionType.Angry;
                    result.RelationshipDelta = -15;
                    break;
            }

            return result;
        }
    }
}
