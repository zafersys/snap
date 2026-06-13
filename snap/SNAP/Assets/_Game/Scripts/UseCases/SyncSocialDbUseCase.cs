using UnityEngine;
using GPOyun.Core;

namespace GPOyun.UseCases
{
    /// <summary>
    /// Encapsulates the Use Case of capturing and syncing simulation data checkpoints into persistent local storage.
    /// </summary>
    public class SyncSocialDbUseCase : MonoBehaviour
    {
        public static SyncSocialDbUseCase Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Execute()
        {
            if (SocialDatabase.Instance == null)
            {
                Debug.LogWarning("[SyncSocialDbUseCase] SocialDatabase is not active!");
                return;
            }

            // Gather all active NPC characters in the simulation
            NPC.NPCController[] npcs = Object.FindObjectsByType<NPC.NPCController>();
            RelationshipMatrix matrix = RelationshipMatrix.Instance;

            if (npcs == null || npcs.Length == 0)
            {
                Debug.LogWarning("[SyncSocialDbUseCase] No active NPCs found to snapshot!");
                return;
            }

            // Sync simulation data and flush to local JSON file
            SocialDatabase.Instance.SyncFromSimulation(npcs, matrix);
            SocialDatabase.Instance.SaveToDisk();
            
            Debug.Log($"[SyncSocialDbUseCase] Successfully synchronized social databases containing {npcs.Length} citizens.");
        }
    }
}
