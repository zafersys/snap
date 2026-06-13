using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPOyun.Core
{
    [Serializable]
    public class ObservationLog
    {
        public string Timestamp;
        public string Description;
        public Color ThreadColor;

        public ObservationLog(string timestamp, string description, Color threadColor)
        {
            Timestamp = timestamp;
            Description = description;
            ThreadColor = threadColor;
        }
    }

    /// <summary>
    /// Stores cozy, observational logging records regarding dynamic social interactions.
    /// Acts as the data store for player observations.
    /// </summary>
    public class JournalManager : MonoBehaviour
    {
        public static JournalManager Instance { get; private set; }

        private readonly List<ObservationLog> _logs = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddObservation(string description, Color threadColor)
        {
            string timeStr = "Day 1, Morning";
            if (Managers.TimeManager.Instance != null)
            {
                timeStr = $"Phase {Managers.TimeManager.Instance.CurrentPhase} ({Managers.TimeManager.Instance.GetFormattedTime()})";
            }

            var log = new ObservationLog(timeStr, description, threadColor);
            _logs.Add(log);

            Debug.Log($"[Journal Logged] {timeStr}: {description}");

            // Clamp max logs to prevent UI overcrowding (optional, cozy limit)
            if (_logs.Count > 40)
            {
                _logs.RemoveAt(0);
            }
        }

        public List<ObservationLog> GetAllLogs()
        {
            return _logs;
        }

        public void ClearLogs()
        {
            _logs.Clear();
        }
    }
}
