using UnityEngine;
using System;

namespace GPOyun.NPC.Data
{
    [Serializable]
    public struct MemoryEvent
    {
        public float Timestamp;
        public StimulusType Trigger;
        public int SourceId;
        public EmotionType FeltEmotion;

        public MemoryEvent(float timestamp, StimulusType trigger, int sourceId, EmotionType feltEmotion)
        {
            Timestamp = timestamp;
            Trigger = trigger;
            SourceId = sourceId;
            FeltEmotion = feltEmotion;
        }
    }

    [Serializable]
    public struct PermanentBelief
    {
        public int TargetNpcId;
        public string NarrativeBias;
        public int StandingOffset;

        public PermanentBelief(int targetId, string bias, int offset)
        {
            TargetNpcId = targetId;
            NarrativeBias = bias;
            StandingOffset = offset;
        }
    }
}
