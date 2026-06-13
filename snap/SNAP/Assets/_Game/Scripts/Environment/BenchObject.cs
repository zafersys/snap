using UnityEngine;
using GPOyun.NPC;

namespace GPOyun.Environment
{
    /// <summary>
    /// Interaction point for NPCs to sit down.
    /// Tracks occupancy and provides sit positions.
    /// </summary>
    public class BenchObject : MonoBehaviour
    {
        [Header("Settings")]
        public Transform[] sitPoints; // Optional: specific spots on the bench
        
        private NPCController _occupant;

        public bool IsOccupied => _occupant != null;

        public bool TryOccupy(NPCController npc)
        {
            if (IsOccupied) return false;
            _occupant = npc;
            return true;
        }

        public void Vacate()
        {
            _occupant = null;
        }

        public Vector3 GetSitPosition()
        {
            // Default to center if no sit points defined
            return transform.position + Vector3.up * 0.5f;
        }
    }
}
