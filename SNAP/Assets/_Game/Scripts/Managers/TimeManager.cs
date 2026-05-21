using UnityEngine;
using GPOyun.Newspaper;
using GPOyun.NPC;

namespace GPOyun.Managers
{
    public enum DayPhase { Morning, Midday, Afternoon, Evening, Night }

    /// <summary>
    /// A1 Level Time Manager
    /// Handles day phases and uses direct calls instead of EventBus.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Settings")]
        public float phaseDurationSeconds = 120f; // 2 minutes per phase
        public bool autoAdvance = true;

        public DayPhase CurrentPhase { get; private set; } = DayPhase.Morning;
        private float _phaseTimer;
        private float _currentHour = 6f; // Starts at 6 AM

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Initial call
            NotifySystemsOfPhaseChange();
        }

        private void Update()
        {
            if (!autoAdvance) return;

            _phaseTimer += Time.deltaTime;
            
            float phaseCompletion = _phaseTimer / phaseDurationSeconds;
            float phaseLengthHours = (CurrentPhase == DayPhase.Night) ? 8f : 4f;
            
            float startHour = GetStartHourForPhase(CurrentPhase);
            _currentHour = (startHour + (phaseCompletion * phaseLengthHours)) % 24f;

            if (_phaseTimer >= phaseDurationSeconds)
            {
                AdvancePhase();
            }
        }

        private float GetStartHourForPhase(DayPhase phase) => phase switch {
            DayPhase.Morning => 6f,
            DayPhase.Midday => 10f,
            DayPhase.Afternoon => 14f,
            DayPhase.Evening => 18f,
            DayPhase.Night => 22f,
            _ => 6f
        };

        public void AdvancePhase()
        {
            _phaseTimer = 0f;
            
            int nextPhase = ((int)CurrentPhase + 1) % 5;
            CurrentPhase = (DayPhase)nextPhase;

            Debug.Log($"[TimeManager] Hour: {GetFormattedTime()}, Phase: {CurrentPhase}");

            NotifySystemsOfPhaseChange();
        }

        private void NotifySystemsOfPhaseChange()
        {
            // Notify Newspaper Manager directly
            if (CurrentPhase == DayPhase.Morning && NewspaperManager.Instance != null)
            {
                NewspaperManager.Instance.OnMorningArrived();
            }

            // Notify all NPCs directly
            if (NPCManager.Instance != null)
            {
                foreach (var npc in NPCManager.Instance.GetAll())
                {
                    npc.OnPhaseChanged(CurrentPhase);
                }
            }
        }

        public string GetFormattedTime()
        {
            int hours = Mathf.FloorToInt(_currentHour);
            int minutes = Mathf.FloorToInt((_currentHour - hours) * 60f);
            return $"{hours:00}:{minutes:00}";
        }

        public float GetCurrentHour() => _currentHour;
        public float GetPhaseProgress() => _phaseTimer / phaseDurationSeconds;
    }
}
