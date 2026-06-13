using UnityEngine;

namespace GPOyun.NPC.Data
{
    public enum StimulusType
    {
        CameraShutter,
        CameraFlash,
        PlayerAimedCamera,
        FriendlyGreeting,
        HostileConfrontation
    }

    public struct Stimulus
    {
        public StimulusType Type;
        public Vector3 Location;
        public int SourceId; // 0 for Player, NpcId for others
        public float Intensity;

        public Stimulus(StimulusType type, Vector3 location, int sourceId, float intensity = 1f)
        {
            Type = type;
            Location = location;
            SourceId = sourceId;
            Intensity = intensity;
        }
    }
}
