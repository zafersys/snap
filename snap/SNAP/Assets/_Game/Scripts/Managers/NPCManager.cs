using System.Collections.Generic;
using UnityEngine;

namespace GPOyun.NPC
{
    /// <summary>
    /// Registry of all active NPCs. Provides lookup and batch operations.
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }
        private readonly List<NPCController> _npcs = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Register(NPCController npc)   => _npcs.Add(npc);
        public void Unregister(NPCController npc) => _npcs.Remove(npc);
        public IReadOnlyList<NPCController> GetAll() => _npcs;
    }
}
