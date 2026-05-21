using UnityEngine;

namespace GPOyun.NPC
{
    /// <summary>
    /// Helper to assign a unique color to an NPC capsule based on their ID.
    /// This makes characters distinguishable in the proto-town.
    /// </summary>
    public class NPCVisualHelper : MonoBehaviour
    {
        [SerializeField] private NPCController controller;
        [SerializeField] private Renderer capsuleRenderer;
        
        private Color _baseColor;

        private void Start()
        {
            if (controller == null) controller = GetComponent<NPCController>();
            if (capsuleRenderer == null) capsuleRenderer = GetComponentInChildren<Renderer>();

            // Generate a base color based on NpcId to keep it consistent
            float hue = (controller != null) ? (controller.NpcId * 0.13f) % 1.0f : 0f;
            _baseColor = Color.HSVToRGB(hue, 0.7f, 0.8f);

            if (capsuleRenderer != null)
            {
                capsuleRenderer.material = new Material(Shader.Find("Standard"));
                capsuleRenderer.material.color = _baseColor;
            }
        }

        private void Update()
        {
            if (capsuleRenderer == null || controller == null) return;

            Color targetColor = _baseColor;

            switch (controller.currentEmotion)
            {
                case EmotionType.Happy:
                    targetColor = Color.yellow;
                    break;
                case EmotionType.Sad:
                    targetColor = Color.blue;
                    break;
                case EmotionType.Angry:
                    targetColor = Color.red;
                    break;
                case EmotionType.Fearful:
                    targetColor = new Color(0.5f, 0f, 0.5f); // Purple
                    break;
                case EmotionType.Surprised:
                    targetColor = Color.cyan;
                    break;
                case EmotionType.Disgusted:
                    targetColor = Color.green;
                    break;
                case EmotionType.Neutral:
                default:
                    targetColor = _baseColor;
                    break;
            }

            capsuleRenderer.material.color = Color.Lerp(capsuleRenderer.material.color, targetColor, Time.deltaTime * 5f);
        }
    }
}
