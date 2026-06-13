using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace GPOyun.Core
{
    [System.Serializable]
    public class NpcStateData
    {
        public int NpcId;
        public string NpcName;
        public string CurrentState;
        public string ActiveBehavior;
        public float Agreeableness;
        public float Extraversion;
    }

    [System.Serializable]
    public class RelationEntry
    {
        public int NpcIdA;
        public int NpcIdB;
        public int RelationshipScore;
    }

    [System.Serializable]
    public class SocialDbModel
    {
        public List<NpcStateData> NpcStates = new();
        public List<RelationEntry> Relationships = new();
    }

    /// <summary>
    /// Decoupled database manager handling persistent disk serialization of social states.
    /// </summary>
    public class SocialDatabase : MonoBehaviour
    {
        public static SocialDatabase Instance { get; private set; }

        private string _savePath;
        private SocialDbModel _model = new();

        public SocialDbModel Model => _model;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            
            _savePath = Path.Combine(Application.persistentDataPath, "social_db.json");
            Debug.Log($"[SocialDatabase] Initialized. Save path: {_savePath}");
            LoadFromDisk();
        }

        public void SyncFromSimulation(NPC.NPCController[] npcs, RelationshipMatrix matrix)
        {
            _model.NpcStates.Clear();
            foreach (var npc in npcs)
            {
                _model.NpcStates.Add(new NpcStateData
                {
                    NpcId = npc.NpcId,
                    NpcName = npc.NpcName,
                    CurrentState = npc.currentState.ToString(),
                    ActiveBehavior = npc.currentState.ToString(),
                    Agreeableness = npc.personality != null ? npc.personality.Agreeableness : 0.5f,
                    Extraversion = npc.personality != null ? npc.personality.Extraversion : 0.5f
                });
            }

            _model.Relationships.Clear();
            if (matrix != null)
            {
                for (int i = 0; i < npcs.Length; i++)
                {
                    for (int j = i + 1; j < npcs.Length; j++)
                    {
                        int idA = npcs[i].NpcId;
                        int idB = npcs[j].NpcId;
                        int score = matrix.GetRelationship(idA, idB);
                        _model.Relationships.Add(new RelationEntry
                        {
                            NpcIdA = idA,
                            NpcIdB = idB,
                            RelationshipScore = score
                        });
                    }
                }
            }
        }

        public void SaveToDisk()
        {
            try
            {
                string json = JsonUtility.ToJson(_model, true);
                File.WriteAllText(_savePath, json);
                Debug.Log($"[SocialDatabase] Saved state database containing {_model.NpcStates.Count} NPCs and {_model.Relationships.Count} relationships.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SocialDatabase] Failed to save database: {ex.Message}");
            }
        }

        public void LoadFromDisk()
        {
            if (!File.Exists(_savePath))
            {
                Debug.Log("[SocialDatabase] No save file found. Ready to compose first snapshot.");
                return;
            }

            try
            {
                string json = File.ReadAllText(_savePath);
                _model = JsonUtility.FromJson<SocialDbModel>(json);
                Debug.Log($"[SocialDatabase] Loaded state database containing {_model.NpcStates.Count} NPCs and {_model.Relationships.Count} relationships.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SocialDatabase] Failed to load database: {ex.Message}");
            }
        }
    }
}
