using UnityEngine;
using System.Collections.Generic;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.Memory
{
    public class NPCMemoryStream : MonoBehaviour
    {
        private List<MemoryEvent> _shortTermMemories = new List<MemoryEvent>();
        private List<PermanentBelief> _permanentBeliefs = new List<PermanentBelief>();

        public void AddMemory(MemoryEvent evt)
        {
            _shortTermMemories.Add(evt);
            
            // Limit short-term memory to prevent unbounded growth
            if (_shortTermMemories.Count > 50)
            {
                _shortTermMemories.RemoveAt(0);
            }
        }

        public void SynthesizeNightlyReflections()
        {
            if (_shortTermMemories.Count == 0) return;

            // In a full implementation, this would batch memories into an LLM prompt.
            // For now, we simulate reflection by generating beliefs based on frequency.
            int playerCameraCount = 0;
            
            foreach (var mem in _shortTermMemories)
            {
                if (mem.SourceId == 0 && (mem.Trigger == StimulusType.PlayerAimedCamera || mem.Trigger == StimulusType.CameraShutter))
                {
                    playerCameraCount++;
                }
            }

            if (playerCameraCount >= 3)
            {
                _permanentBeliefs.Add(new PermanentBelief(0, "The photographer is obsessed with me and keeps stalking me.", -40));
            }

            _shortTermMemories.Clear();
        }

        public IReadOnlyList<PermanentBelief> GetBeliefs()
        {
            return _permanentBeliefs;
        }
    }
}
