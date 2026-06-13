using UnityEngine;
using UnityEngine.UI;
using GPOyun.Newspaper;

namespace GPOyun.UI
{
    /// <summary>
    /// A1 Level HUD Manager
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("UI Overlays")]
        public GameObject viewfinderGroup;
        public Text photoCountText;
        public Text clockText;

        [Header("Animation Settings")]
        public float pulseSpeed = 2f;
        public float pulseAmount = 0.1f;

        public static HUDManager Instance { get; private set; }

        [Header("Overlay Settings")]
        public bool relationshipOverlayActive = false;

        private int _totalPhotos = 5;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(GameObject viewfinder, Text photoText, Text clock)
        {
            viewfinderGroup = viewfinder;
            photoCountText = photoText;
            clockText = clock;
        }

        private void Start()
        {
            if (viewfinderGroup != null) viewfinderGroup.SetActive(false);
            UpdatePhotoUI();
        }

        private readonly System.Collections.Generic.List<EmojiReaction> _activeReactions = new();

        public void SpawnEmojiReaction(Transform targetNpc, string symbol, Color color)
        {
            string mappedSymbol = symbol;
            
            // Map bracket labels to rich Unicode Emojis
            if (symbol == "[FLAME]") mappedSymbol = "🔥 Rivalry!";
            else if (symbol == "[STAR]") mappedSymbol = "⭐ Hero!";
            else if (symbol == "[GIFT]") mappedSymbol = "🎁 Gift!";
            else if (symbol == "[PIZZA]") mappedSymbol = "🍕 Pizza!";
            else if (symbol == "[ZZZ]") mappedSymbol = "💤 Snoozing";
            else if (symbol == "[SHH]") mappedSymbol = "🤫 Gossip!";
            else if (symbol == "[DRAMA]") mappedSymbol = "🎭 Drama!";
            else if (symbol == "[CELEB]") mappedSymbol = "🎉 Celebration!";
            else if (symbol == "[BROKEN]") mappedSymbol = "💔 Betrayal";
            else if (symbol == "[ALLY]") mappedSymbol = "🤝 Alliance";
            else if (symbol == "[CHILL]") mappedSymbol = "🕶️ Chilling";
            else if (symbol == "[TRAVEL]") mappedSymbol = "🎒 Traveling";
            else if (symbol == "[TALK]") mappedSymbol = "💬 Chatting";
            else if (symbol == "[FLEE]") mappedSymbol = "🏃 Fleeing";
            else if (symbol == "[HAPPY]") mappedSymbol = "😊 Happy";
            else if (symbol == "[SAD]") mappedSymbol = "😢 Sad";
            else if (symbol == "[ANGRY]") mappedSymbol = "😡 Angry";

            _activeReactions.Add(new EmojiReaction
            {
                target = targetNpc,
                symbol = mappedSymbol,
                color = color,
                timeStarted = Time.time,
                driftOffset = new Vector2(Random.Range(-30f, 30f), Random.Range(30f, 60f))
            });
        }

        private void DrawActiveEmojiReactions()
        {
            var mainCam = Camera.main;
            if (mainCam == null) return;

            for (int i = _activeReactions.Count - 1; i >= 0; i--)
            {
                var rx = _activeReactions[i];
                if (rx.target == null) { _activeReactions.RemoveAt(i); continue; }

                float elapsed = Time.time - rx.timeStarted;
                if (elapsed >= rx.duration)
                {
                    _activeReactions.RemoveAt(i);
                    continue;
                }

                // Project head to screen
                Vector3 worldPos = rx.target.position + Vector3.up * 2.5f;
                Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

                if (screenPos.z <= 0) continue;

                // Float upward and drift
                float progress = elapsed / rx.duration;
                float x = screenPos.x;
                float y = (Screen.height - screenPos.y) - rx.driftOffset.y * progress - (progress * 100f);

                GUIStyle emojiStyle = new GUIStyle();
                emojiStyle.fontSize = (int)(32f * (1.5f - progress)); // Balanced drift size
                emojiStyle.fontStyle = FontStyle.Bold;
                emojiStyle.alignment = TextAnchor.MiddleCenter;

                // Dynamic fade opacity
                Color fontColor = rx.color;
                fontColor.a = 1f - progress;

                // Draw text shadow
                emojiStyle.normal.textColor = new Color(0, 0, 0, 1f - progress);
                GUI.Label(new Rect(x - 150 + 1, y - 20 + 1, 300, 40), rx.symbol, emojiStyle);

                // Draw text
                emojiStyle.normal.textColor = fontColor;
                GUI.Label(new Rect(x - 150, y - 20, 300, 40), rx.symbol, emojiStyle);
            }
        }

        private void DrawCozyScoreboard()
        {
            var npcs = FindObjectsByType<NPC.NPCController>();
            if (npcs == null || npcs.Length == 0) return;

            int width = 750;
            int height = 350;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            // 1. Semi-transparent high-density backdrop
            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0.04f, 0.04f, 0.06f, 0.95f));
            bgTex.Apply();

            GUIStyle panelStyle = new GUIStyle();
            panelStyle.normal.background = bgTex;
            GUI.Box(new Rect(x, y, width, height), "", panelStyle);

            // 2. Santorini Gold Header
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.9f, 0.75f, 0.25f); // Santorini Gold
            titleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y + 20, width, 35), "SANTORINI SOCIAL STANDINGS SCOREBOARD", titleStyle);

            GUIStyle subtitleStyle = new GUIStyle();
            subtitleStyle.fontSize = 13;
            subtitleStyle.normal.textColor = Color.gray;
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y + 50, width, 20), "Live social standings, active emotional states, and proximity indices", subtitleStyle);

            // 3. Grid Columns Headers
            float startY = y + 80;
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleLeft;

            GUI.Label(new Rect(x + 50, startY, 180, 25), "CHARACTER NAME", headerStyle);
            GUI.Label(new Rect(x + 240, startY, 130, 25), "ACTIVE EMOTION", headerStyle);
            GUI.Label(new Rect(x + 390, startY, 150, 25), "RELATION INDEX", headerStyle);
            GUI.Label(new Rect(x + 560, startY, 150, 25), "COZY STANDING", headerStyle);

            // Horizontal Separator Line
            Texture2D lineTex = new Texture2D(1, 1);
            lineTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.15f));
            lineTex.Apply();
            GUIStyle lineStyle = new GUIStyle();
            lineStyle.normal.background = lineTex;
            GUI.Box(new Rect(x + 50, startY + 28, width - 100, 2), "", lineStyle);

            // 4. Populate rows for each NPC
            float rowY = startY + 38;
            GUIStyle rowStyle = new GUIStyle();
            rowStyle.fontSize = 14;
            rowStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            rowStyle.alignment = TextAnchor.MiddleLeft;

            foreach (var npc in npcs)
            {
                int bestScore = 0;
                bool hasBonds = false;
                foreach (var other in npcs)
                {
                    if (other == npc) continue;
                    int score = Core.RelationshipMatrix.Instance != null ? Core.RelationshipMatrix.Instance.GetRelationship(npc.NpcId, other.NpcId) : 0;
                    if (Mathf.Abs(score) > Mathf.Abs(bestScore))
                    {
                        bestScore = score;
                        hasBonds = true;
                    }
                }

                string status = "Stranger";
                Color statusColor = Color.gray;

                if (hasBonds)
                {
                    if (bestScore >= 86)
                    {
                        status = "★ BEST FRIEND";
                        statusColor = new Color(0.9f, 0.75f, 0.25f); // Santorini Gold
                    }
                    else if (bestScore >= 50)
                    {
                        status = "Friend";
                        statusColor = new Color(0.2f, 0.6f, 0.9f); // Cobalt Blue
                    }
                    else if (bestScore <= -50)
                    {
                        status = "☠ RIVAL";
                        statusColor = new Color(0.85f, 0.35f, 0.2f); // Terracotta Red
                    }
                }

                // Render fields
                GUI.Label(new Rect(x + 50, rowY, 180, 25), npc.gameObject.name, rowStyle);
                GUI.Label(new Rect(x + 240, rowY, 130, 25), npc.currentEmotion.ToString().ToUpper(), rowStyle);
                GUI.Label(new Rect(x + 390, rowY, 150, 25), $"{bestScore} pts", rowStyle);

                GUIStyle curStatusStyle = new GUIStyle(rowStyle);
                curStatusStyle.normal.textColor = statusColor;
                curStatusStyle.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(x + 560, rowY, 150, 25), status, curStatusStyle);

                rowY += 30;
            }
        }

        private void OnGUI()
        {
            DrawActiveEmojiReactions();

            if (!relationshipOverlayActive) return;

            // Draw Esports Scoreboard
            DrawCozyScoreboard();

            var mainCam = Camera.main;
            if (mainCam == null) return;

            var npcs = FindObjectsByType<NPC.NPCController>();
            if (npcs == null) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 15;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            foreach (var npc in npcs)
            {
                // Position slightly above the NPC head (e.g. y = 2.2f)
                Vector3 worldPos = npc.transform.position + Vector3.up * 2.4f;
                Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

                // Check if behind the camera
                if (screenPos.z <= 0) continue;

                // Project to Screen
                float x = screenPos.x;
                // GUI y starts from top-left, while screenPos y starts from bottom-left
                float y = Screen.height - screenPos.y;

                // Let's find their closest neighbor and active relationship score
                Color color = Color.white;

                int bestScore = 0;
                bool hasBonds = false;

                // Find relationship score with other NPCs
                foreach (var other in npcs)
                {
                    if (other == npc) continue;
                    int score = Core.RelationshipMatrix.Instance != null ? Core.RelationshipMatrix.Instance.GetRelationship(npc.NpcId, other.NpcId) : 0;
                    if (Mathf.Abs(score) > Mathf.Abs(bestScore))
                    {
                        bestScore = score;
                        hasBonds = true;
                    }
                }

                string status = "Stranger";
                color = Color.white; // Strangers

                if (hasBonds)
                {
                    if (bestScore >= 86)
                    {
                        status = "★ BEST FRIEND";
                        color = new Color(1f, 0.82f, 0.22f); // Gold
                    }
                    else if (bestScore >= 50)
                    {
                        status = "Friend";
                        color = new Color(0.2f, 0.6f, 0.9f); // Cobalt Blue
                    }
                    else if (bestScore <= -50)
                    {
                        status = "☠ RIVAL";
                        color = new Color(0.85f, 0.35f, 0.2f); // Terracotta Red
                    }
                }

                // Determine dynamic Player Status label and color
                string playerStatus = "Stranger";
                Color playerColor = Color.white;
                if (npc.relationshipWithPlayer >= 50)
                {
                    playerStatus = "★ ALLY / ADMIRER";
                    playerColor = new Color(1f, 0.45f, 0.65f); // Beautiful Soft Pink
                }
                else if (npc.relationshipWithPlayer <= -50)
                {
                    playerStatus = "☠ HOSTILE / RIVAL";
                    playerColor = new Color(0.85f, 0.28f, 0.28f); // High-contrast Red
                }

                string label = $"{npc.NpcName} ({npc.currentEmotion.ToString().ToUpper()})\nSocial: {status} ({bestScore})\nOpinion of You: {playerStatus} ({npc.relationshipWithPlayer})";

                // Draw background shadow
                style.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Label(new Rect(x - 200 + 1, y - 40 + 1, 400, 80), label, style);

                // Draw text
                style.normal.textColor = playerColor != Color.white ? playerColor : color;
                GUI.Label(new Rect(x - 200, y - 40, 400, 80), label, style);
            }
        }

        private void Update()
        {
            HandleAimingVisuals();
            UpdateClockUI();
            UpdatePhotoUI();
        }

        private void UpdateClockUI()
        {
            if (clockText != null && GPOyun.Managers.TimeManager.Instance != null)
            {
                clockText.text = GPOyun.Managers.TimeManager.Instance.GetFormattedTime();
            }
        }

        private void HandleAimingVisuals()
        {
            bool isAiming = false;
            var camCtrl = Object.FindAnyObjectByType<CameraSystem.CameraController>();
            if (camCtrl != null)
            {
                isAiming = camCtrl.IsViewfinderActive();
            }
            else
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                isAiming = keyboard != null && keyboard.cKey.isPressed;
            }
            
            if (viewfinderGroup != null)
            {
                viewfinderGroup.SetActive(isAiming);
                
                if (isAiming)
                {
                    float scale = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                    viewfinderGroup.transform.localScale = Vector3.one * scale;
                }
            }
        }

        private void UpdatePhotoUI()
        {
            if (photoCountText != null && NewspaperManager.Instance != null)
            {
                int photosTaken = NewspaperManager.Instance.GetTodaysPhotos().Count;
                photoCountText.text = $"PHOTOS: {photosTaken} / {_totalPhotos}";
            }
        }
    }

    public class EmojiReaction
    {
        public Transform target;
        public string symbol;
        public Color color;
        public float timeStarted;
        public float duration = 1.5f;
        public Vector2 driftOffset;
    }
}
