using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GPOyun.Core;

namespace GPOyun.UI
{
    /// <summary>
    /// Cozy observational journal UI stack. Opens with [J] key.
    /// Lists observed social logs on the right, and active relationship bonds on the left.
    /// Built completely procedurally (Zero-Asset) to ensure seamless setup.
    /// </summary>
    public class JournalUI : MonoBehaviour
    {
        private static JournalUI _instance;
        public static JournalUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<JournalUI>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        [Header("References")]
        public CanvasGroup journalCanvasGroup;
        public Text relationshipListText;
        public Text observationLogsText;

        private bool _isOpen = false;
        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void Start()
        {
            SetupUI();
            Hide();
        }

        // Input listening handled centrally by GlobalInputListener

        public void Toggle()
        {
            if (_isOpen) Hide();
            else Show();
        }

        public void Show()
        {
            _isOpen = true;
            SettingsController.Instance?.Hide();
            PhotoGalleryUI.Instance?.Hide();
            EditorialUI.Instance?.Hide();
            NewspaperBoardUI.Instance?.Hide();

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.PauseGame();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            RefreshData();
            Apply(1f, true, true);
            Debug.Log("[JournalUI] Opened.");
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (Core.GameManager.Instance != null) Core.GameManager.Instance.ResumeGame();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Apply(0f, false, false);
            Debug.Log("[JournalUI] Closed.");
        }

        private Coroutine _fadeCoroutine;

        private void Apply(float targetAlpha, bool blocks, bool interact)
        {
            if (journalCanvasGroup == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            if (Application.isBatchMode)
            {
                journalCanvasGroup.alpha = targetAlpha;
                journalCanvasGroup.blocksRaycasts = blocks;
                journalCanvasGroup.interactable = interact;
            }
            else
            {
                _fadeCoroutine = StartCoroutine(VisualUtils.FadeGroupCoroutine(journalCanvasGroup, targetAlpha, 0.15f, blocks, interact));
            }
        }

        private void RefreshData()
        {
            // 1. Build relationship connections
            if (relationshipListText != null && RelationshipMatrix.Instance != null)
            {
                var npcs = FindObjectsByType<NPC.NPCController>();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<b>[ VILLAGE BONDS ]</b>");
                sb.AppendLine("-----------------------------------------");

                if (npcs.Length < 2)
                {
                    sb.AppendLine("\n[ NO CITIZENS FOUND ]");
                }
                else
                {
                    foreach (var npc in npcs)
                    {
                        var loves = new List<string>();
                        var friends = new List<string>();
                        var rivals = new List<string>();

                        foreach (var other in npcs)
                        {
                            if (npc == other) continue;
                            int score = RelationshipMatrix.Instance.GetRelationship(npc.NpcId, other.NpcId);
                            
                            if (score >= 86) loves.Add($"{other.NpcName} ({score})");
                            else if (score >= 50) friends.Add($"{other.NpcName} ({score})");
                            else if (score <= -50) rivals.Add($"{other.NpcName} ({score})");
                        }

                        if (loves.Count > 0 || friends.Count > 0 || rivals.Count > 0)
                        {
                            sb.AppendLine($"<color=#E6C03C><b>{npc.NpcName}</b></color>");
                            if (loves.Count > 0) sb.AppendLine($"  <color=#FF69B4>Loves: </color> {string.Join(", ", loves)}");
                            if (friends.Count > 0) sb.AppendLine($"  <color=#327CB2>Friends:</color> {string.Join(", ", friends)}");
                            if (rivals.Count > 0) sb.AppendLine($"  <color=#D95933>Rivals: </color> {string.Join(", ", rivals)}");
                            sb.AppendLine();
                        }
                    }

                    if (sb.Length < 100) // Rough check if nothing was added
                    {
                        sb.AppendLine("\n[ NO STRONG BONDS FORMED YET ]");
                    }
                }
                relationshipListText.text = sb.ToString();
            }

            // 2. Build Observation Logs
            if (observationLogsText != null && JournalManager.Instance != null)
            {
                var logs = JournalManager.Instance.GetAllLogs();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("<b>[ SOCIAL TIMELINE ]</b>");
                sb.AppendLine("-----------------------------------------");

                if (logs.Count == 0)
                {
                    sb.AppendLine("\n[ NO OBSERVATIONS YET ]");
                }
                else
                {
                    // Print logs in reverse chronological order
                    for (int i = logs.Count - 1; i >= 0; i--)
                    {
                        var log = logs[i];
                        string colorTag = "#" + ColorUtility.ToHtmlStringRGB(log.ThreadColor);
                        sb.AppendLine($"<color={colorTag}>[*]</color> {log.Description}");
                    }
                }
                observationLogsText.text = sb.ToString();
            }
        }

        private void SetupUI()
        {
            Canvas canvas = VisualUtils.CreateBaseCanvas("JOURNAL_CANVAS", 800, transform);
            journalCanvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            journalCanvasGroup.alpha = 0f;
            journalCanvasGroup.blocksRaycasts = false;
            journalCanvasGroup.interactable = false;

            // Dark semi-transparent background
            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.04f, 0.06f, 0.96f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(canvas.transform, false);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 40; titleTxt.color = VisualUtils.StuccoWhite;
            titleTxt.text = "OBSERVATIONAL JOURNAL  [J]";
            titleTxt.alignment = TextAnchor.MiddleCenter;
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1); titleRect.anchorMax = new Vector2(1, 1);
            titleRect.anchoredPosition = new Vector2(0, -45); titleRect.sizeDelta = new Vector2(0, 60);

            // Left panel: Relationship Threads
            GameObject leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(canvas.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.02f, 0.05f); leftRect.anchorMax = new Vector2(0.48f, 0.85f);
            leftRect.offsetMin = leftRect.offsetMax = Vector2.zero;
            
            relationshipListText = leftPanel.AddComponent<Text>();
            relationshipListText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            relationshipListText.fontSize = 20;
            relationshipListText.color = Color.white;
            relationshipListText.alignment = TextAnchor.UpperLeft;
            relationshipListText.supportRichText = true;

            // Right panel: Observation Logs
            GameObject rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(canvas.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.52f, 0.05f); rightRect.anchorMax = new Vector2(0.98f, 0.85f);
            rightRect.offsetMin = rightRect.offsetMax = Vector2.zero;

            observationLogsText = rightPanel.AddComponent<Text>();
            observationLogsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            observationLogsText.fontSize = 20;
            observationLogsText.color = Color.white;
            observationLogsText.alignment = TextAnchor.UpperLeft;
            observationLogsText.supportRichText = true;

            VisualUtils.EnsureCanvasRenderers(transform);
        }
    }
}
