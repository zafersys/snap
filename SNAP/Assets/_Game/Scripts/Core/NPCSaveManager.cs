using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GPOyun.NPC;
using GPOyun.NPC.Data;
using GPOyun.NPC.Memory;

namespace GPOyun.Core
{
    [System.Serializable]
    public class WorldSaveData
    {
        // For serialization of dictionaries, we often use parallel arrays or custom structs, 
        // but for simplicity in this architectural slice we will serialize relationship pairs.
        public List<RelationshipData> Relationships = new List<RelationshipData>();
        public List<NPCMemorySaveData> NpcMemories = new List<NPCMemorySaveData>();
    }

    [System.Serializable]
    public class RelationshipData
    {
        public int IdA;
        public int IdB;
        public int Score;
    }

    [System.Serializable]
    public class NPCMemorySaveData
    {
        public int NpcId;
        public List<MemoryEvent> ShortTermMemories = new List<MemoryEvent>();
        public List<string> Instincts = new List<string>();
    }

    /// <summary>
    /// Handles persisting the RelationshipMatrix and NPC Memory Streams to a JSON file.
    /// </summary>
    public class NPCSaveManager : MonoBehaviour
    {
        public static NPCSaveManager Instance { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, "PersistentWorldState.json");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            LoadWorldState();
        }

        private void OnApplicationQuit()
        {
            SaveWorldState();
        }

        public void SaveWorldState()
        {
            WorldSaveData data = new WorldSaveData();

            // 1. Gather Relationships
            if (RelationshipMatrix.Instance != null)
            {
                // In a real implementation we would iterate the internal _relationsTable
                // Since it's private in the existing code, we will iterate all NPC pairs dynamically
                var npcs = FindObjectsByType<NPCController>(FindObjectsInactive.Exclude);
                for (int i = 0; i < npcs.Length; i++)
                {
                    for (int j = i + 1; j < npcs.Length; j++)
                    {
                        int idA = npcs[i].NpcId;
                        int idB = npcs[j].NpcId;
                        int score = RelationshipMatrix.Instance.GetRelationship(idA, idB);
                        if (score != 0)
                        {
                            data.Relationships.Add(new RelationshipData { IdA = idA, IdB = idB, Score = score });
                        }
                    }
                }

                // 2. Gather Memories and Instincts
                foreach (var npc in npcs)
                {
                    var memStream = npc.GetComponent<NPCMemoryStream>();
                    if (memStream != null)
                    {
                        var npcData = new NPCMemorySaveData
                        {
                            NpcId = npc.NpcId,
                            ShortTermMemories = memStream.GetMemorySnapshot(),
                            Instincts = memStream.GetInstincts()
                        };
                        data.NpcMemories.Add(npcData);
                    }
                }
            }

            // Write to Disk
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveManager] World State saved to: {SavePath}");
        }

        public void LoadWorldState()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[SaveManager] No existing save found. Starting fresh.");
                return;
            }

            string json = File.ReadAllText(SavePath);
            WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);

            // 1. Restore Relationships
            if (RelationshipMatrix.Instance != null)
            {
                RelationshipMatrix.Instance.InitializeMatrix(); // Clear first
                foreach (var rel in data.Relationships)
                {
                    RelationshipMatrix.Instance.ModifyRelationship(rel.IdA, rel.IdB, rel.Score);
                }
            }

            // 2. Restore Memories and Instincts
            var npcs = FindObjectsByType<NPCController>(FindObjectsInactive.Exclude);
            foreach (var npc in npcs)
            {
                var npcData = data.NpcMemories.Find(d => d.NpcId == npc.NpcId);
                if (npcData != null)
                {
                    var memStream = npc.GetComponent<NPCMemoryStream>();
                    if (memStream != null)
                    {
                        memStream.RestoreState(npcData.ShortTermMemories, npcData.Instincts);
                    }
                }
            }

            Debug.Log("[SaveManager] World State successfully loaded.");
        }
    }
}
