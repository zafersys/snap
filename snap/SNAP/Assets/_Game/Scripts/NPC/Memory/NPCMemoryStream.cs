using UnityEngine;
using System.Collections.Generic;
using GPOyun.NPC.Data;

namespace GPOyun.NPC.Memory
{
    public class NPCMemoryStream : MonoBehaviour
    {
        private List<MemoryEvent> _shortTermMemories = new List<MemoryEvent>();
        private List<PermanentBelief> _permanentBeliefs = new List<PermanentBelief>();
        private List<string> _instincts = new List<string>();

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
                if (!_instincts.Contains("CameraShy"))
                {
                    _instincts.Add("CameraShy");
                    Debug.Log($"[Memory] {gameObject.name} developed a permanent instinct: CameraShy");
                }
            }

            _shortTermMemories.Clear();
        }

        public bool HasInstinct(string instinct)
        {
            return _instincts.Contains(instinct);
        }

        public List<MemoryEvent> GetMemorySnapshot()
        {
            return new List<MemoryEvent>(_shortTermMemories);
        }

        public List<string> GetInstincts()
        {
            return new List<string>(_instincts);
        }

        public void RestoreState(List<MemoryEvent> memories, List<string> instincts)
        {
            _shortTermMemories = new List<MemoryEvent>(memories);
            _instincts = new List<string>(instincts);
            Debug.Log($"[Memory] Restored state for {gameObject.name} ({_shortTermMemories.Count} memories, {_instincts.Count} instincts).");
        }

        public IReadOnlyList<PermanentBelief> GetBeliefs()
        {
            return _permanentBeliefs;
        }
    }
}
