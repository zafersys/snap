using System.Collections.Generic;
using UnityEngine;

namespace GPOyun.Core
{
    /// <summary>
    /// Decoupled manager that handles pure emoji-only observation formatting and logging history.
    /// </summary>
    public class SocialHistoryLogger : MonoBehaviour
    {
        public static SocialHistoryLogger Instance { get; private set; }

        private readonly List<string> _observations = new();

        public List<string> Observations => _observations;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void LogObservation(string emojiChain)
        {
            if (string.IsNullOrEmpty(emojiChain)) return;

            string stampedChain = $"🕒 {emojiChain}";
            _observations.Add(stampedChain);
            Debug.Log($"[SocialHistoryLogger] Emoji Logged: {stampedChain}");

            // Also mirror to legacy JournalManager so UI panels automatically reflect the changes
            if (JournalManager.Instance != null)
            {
                JournalManager.Instance.AddObservation(emojiChain, new Color(0.85f, 0.85f, 0.85f));
            }
        }

        public string FormatConversation(string speakerName, string listenerName, string emojiDimension)
        {
            return $"👤 {speakerName} {emojiDimension} 👤 {listenerName}";
        }

        public string FormatTravelLog(string characterName, string destinationEmoji)
        {
            return $"👤 {characterName} ✈️ {destinationEmoji} \\o/";
        }
    }
}
