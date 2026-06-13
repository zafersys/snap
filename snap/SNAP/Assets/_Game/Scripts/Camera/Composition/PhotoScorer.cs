using System.Collections.Generic;
using UnityEngine;
using GPOyun.NPC;
using GPOyun.Newspaper;
using GPOyun.Core;

namespace GPOyun.CameraSystem
{
    /// <summary>
    /// Photographic evaluation engine. Analyzes snapshot alignment,
    /// action states, and character proximity to calculate composition scores.
    /// </summary>
    public class PhotoScorer : MonoBehaviour
    {
        public static PhotoScorer Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public PhotoData ScorePhoto(Camera camera, GPOyun.Environment.PhotoSubject mainSubject, Texture2D texture)
        {
            if (camera == null) return null;

            float alignmentScore = 0f;
            float actionBonus = 0f;
            float crowdBonus = 0f;

            string subjectName = "Environment";
            NewsCategory category = NewsCategory.Local;

            // 1. Framing alignment calculation
            if (mainSubject != null)
            {
                subjectName = mainSubject.SubjectName;
                category = mainSubject.PrimaryCategory;

                Vector3 screenPoint = camera.WorldToViewportPoint(mainSubject.transform.position);
                Vector2 centerOffset = new Vector2(screenPoint.x - 0.5f, screenPoint.y - 0.5f);
                float distance = centerOffset.magnitude; // Offset distance from viewport center [0, 0.707]
                
                // Centering metric: perfect center = 100
                alignmentScore = Mathf.Clamp01(1f - (distance / 0.5f)) * 100f;

                // 2. Action capture check
                var controller = mainSubject.GetComponent<NPCController>();
                if (controller != null)
                {
                    actionBonus = GetActionBonus(controller.currentState);
                }
            }
            else
            {
                alignmentScore = Random.Range(10f, 30f); // Default baseline environmental shot
            }

            // 3. Adjacency Crowd Factor calculation
            List<NPCController> nearbyNpcs = GetSecondaryNpcs(mainSubject);
            crowdBonus = Mathf.Min(4, nearbyNpcs.Count) * 10f; // Max crowd bonus is +40

            // Combine factors
            // Alignment: 50% weight + Action: up to +50 points + Crowd: up to +40 points
            int finalScore = Mathf.Clamp(Mathf.RoundToInt((alignmentScore * 0.5f) + actionBonus + crowdBonus), 5, 100);

            // Log social observations based on what was snapped
            LogObservations(mainSubject, nearbyNpcs, actionBonus);

            var photoData = new PhotoData
            {
                CapturedTexture = texture,
                WorldPosition = camera.transform.position,
                PrimarySubject = mainSubject,
                CompositionScore = finalScore
            };

            return photoData;
        }

        private float GetActionBonus(NPCState state)
        {
            return state switch
            {
                NPCState.Hugging => 50f,
                NPCState.Fleeing => 50f,
                NPCState.ChillingInGroup => 25f,
                NPCState.Sitting => 25f,
                _ => 0f
            };
        }

        private List<NPCController> GetSecondaryNpcs(GPOyun.Environment.PhotoSubject mainSubject)
        {
            var secondary = new List<NPCController>();
            if (mainSubject == null) return secondary;

            var npcs = FindObjectsByType<NPCController>();
            foreach (var npc in npcs)
            {
                if (npc.gameObject == mainSubject.gameObject) continue;

                float dist = Vector3.Distance(mainSubject.transform.position, npc.transform.position);
                if (dist < 5.0f) // Snapshot detection radius
                {
                    secondary.Add(npc);
                }
            }

            return secondary;
        }

        private void LogObservations(GPOyun.Environment.PhotoSubject main, List<NPCController> crowd, float actionBonus)
        {
            if (JournalManager.Instance == null || main == null) return;

            var controller = main.GetComponent<NPCController>();
            if (controller == null) return;

            string idStr = controller.NpcId.ToString();

            if (actionBonus >= 50f)
            {
                JournalManager.Instance.AddObservation(
                    $"[CAMERA] -> [NPC_{idStr}] [DRAMA] /!\\",
                    new Color(1f, 0.4f, 0.2f)
                );
            }
            else if (crowd.Count > 1)
            {
                JournalManager.Instance.AddObservation(
                    $"[CAMERA] -> [NPC_{idStr}] [COZY-CLUSTER] x{crowd.Count + 1}",
                    Color.cyan
                );
            }
            else
            {
                JournalManager.Instance.AddObservation(
                    $"[CAMERA] -> [NPC_{idStr}] [CHILL]",
                    Color.white
                );
            }
        }
    }
}
