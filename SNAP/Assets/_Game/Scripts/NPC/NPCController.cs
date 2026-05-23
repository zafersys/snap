using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using GPOyun.Managers;
using GPOyun.Newspaper;
using GPOyun.Core;

namespace GPOyun.NPC
{
    public enum NPCState { Idle, Wandering, WalkingToBoard, Reading, Reacting, WalkingHome, Sitting, Hugging, Fleeing, ChillingInGroup, Socializing, Traveling }
    public enum EmotionType { Neutral, Happy, Sad, Angry, Fearful, Surprised, Disgusted }

    /// <summary>
    /// Human-like NPC Controller.
    /// Wanders the town square, clusters near the fountain at certain times,
    /// reacts to news at the board, and returns home at night.
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        [Header("Identity")]
        public int NpcId;
        public string NpcName = "Citizen";
        public NPCPersonalityData personality;

        [Header("Movement")]
        public Transform boardPosition;
        public float moveSpeed     = 1.8f;
        public float runSpeed      = 3.2f;
        public float turnSpeed     = 4f;

        [Header("State")]
        public NPCState  currentState   = NPCState.Idle;
        public EmotionType currentEmotion = EmotionType.Neutral;

        [Header("Player Relationship")]
        [Range(-100, 100)]
        public int relationshipWithPlayer = 0;

        [Header("Wander Settings")]
        public float wanderRadius   = 10f;
        public float minIdleTime    =  2f;
        public float maxIdleTime    =  7f;
        public float minWanderDist  =  2f;
        public float maxWanderDist  =  8f;

        // Internal
        private Vector3  _homePosition;
        public Vector3 homePosition => _homePosition;
        private Vector3  _targetPosition;
        private Animator _animator;
        private Coroutine _reactCoroutine;
        private NewsPublishedData _pendingNews;
        private bool _hasReadTodayNews;
        private PantomimeGestures _gestures;
        private Environment.BenchObject _occupiedBench;
        private float _socializeCooldownTimer = 0f;

        // Neural Subsystems
        private Sensory.NPCSensoryMatrix _sensoryMatrix;
        private Appraisal.NPCAppraisalEngine _appraisalEngine;
        private Memory.NPCMemoryStream _memoryStream;
        private UtilityAI.NPCNeeds _needs;
        private UtilityAI.NPCActionPlanner _actionPlanner;

        // ─── Unity Events ──────────────────────────────────────────────────

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _homePosition = transform.position;
            
            _gestures = GetComponent<PantomimeGestures>();
            if (_gestures == null) _gestures = gameObject.AddComponent<PantomimeGestures>();

            // Delegate locomotion to Locomotion Component
            var locomotion = GetComponent<NPCLocomotion>();
            if (locomotion == null) gameObject.AddComponent<NPCLocomotion>();

            _sensoryMatrix = gameObject.AddComponent<Sensory.NPCSensoryMatrix>();
            _appraisalEngine = gameObject.AddComponent<Appraisal.NPCAppraisalEngine>();
            _memoryStream = gameObject.AddComponent<Memory.NPCMemoryStream>();
            _needs = gameObject.AddComponent<UtilityAI.NPCNeeds>();
            _actionPlanner = gameObject.AddComponent<UtilityAI.NPCActionPlanner>();

            // Add basic actions
            gameObject.AddComponent<UtilityAI.WanderAction>();
            gameObject.AddComponent<UtilityAI.SocializeAction>();
            gameObject.AddComponent<UtilityAI.FleeAction>();
            gameObject.AddComponent<UtilityAI.GossipAction>();
            gameObject.AddComponent<UtilityAI.SearchAction>();
            gameObject.AddComponent<UtilityAI.TravelAction>();
            gameObject.AddComponent<UtilityAI.ChillAloneAction>();
            gameObject.AddComponent<UtilityAI.ReadNewsAction>();
            gameObject.AddComponent<UtilityAI.GoHomeAction>();
            gameObject.AddComponent<UtilityAI.FleePlayerAction>();
            gameObject.AddComponent<UtilityAI.ArgueAction>();
            gameObject.AddComponent<UtilityAI.ApproachPlayerAction>();
            gameObject.AddComponent<UtilityAI.GroupHangoutAction>();
            // more actions can be added via inspector or dynamically

            _sensoryMatrix.OnStimulusDetected += HandleStimulus;

            CreateLabel();
        }

        private void HandleStimulus(Data.Stimulus stimulus)
        {
            if (_appraisalEngine == null || _memoryStream == null) return;
            
            var result = _appraisalEngine.Evaluate(stimulus, currentEmotion);
            
            if (result.NewEmotion != currentEmotion)
            {
                currentEmotion = result.NewEmotion;
                // If angry, maybe transition state
                if (currentEmotion == EmotionType.Angry)
                {
                    currentState = NPCState.Reacting;
                    TriggerReaction("🤬", VisualUtils.Terracotta);
                    StartCoroutine(ResumeAutonomyAfterPose(3f));
                }
            }

            _memoryStream.AddMemory(new Data.MemoryEvent(Time.time, stimulus.Type, stimulus.SourceId, currentEmotion));
            relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + result.RelationshipDelta, -100, 100);
        }

        private void Start()
        {
            if (NPCManager.Instance != null) NPCManager.Instance.Register(this);
            
            if (_appraisalEngine != null) _appraisalEngine.Initialize(personality, _memoryStream);
            if (_actionPlanner != null) _actionPlanner.Initialize(this, _needs);
            
            // We delegate Wander start to the ActionPlanner now!
        }

        private void OnDestroy()
        {
            if (NPCManager.Instance != null) NPCManager.Instance.Unregister(this);
        }

        private void Update()
        {
            if (_socializeCooldownTimer > 0f)
            {
                _socializeCooldownTimer -= Time.deltaTime;
            }
        }

        // ─── Movement ─────────────────────────────────────────────────────

        // ─── State Machine ────────────────────────────────────────────────

        public void EnterState(NPCState newState)
        {
            currentState = newState;

            // Reset slouched sad/sit scales
            if (newState != NPCState.Sitting)
            {
                transform.localScale = Vector3.one;
            }

            switch (newState)
            {
                case NPCState.Reading:
                    if (boardPosition != null)
                        transform.rotation = Quaternion.LookRotation(boardPosition.position - transform.position);
                    ProcessReadNews();
                    if (_reactCoroutine != null) StopCoroutine(_reactCoroutine);
                    _reactCoroutine = StartCoroutine(ReactRoutine());
                    break;

                case NPCState.WalkingHome:
                    _targetPosition = _homePosition;
                    if (_occupiedBench != null)
                    {
                        _occupiedBench.Vacate();
                        _occupiedBench = null;
                    }
                    break;

                case NPCState.Idle:
                    currentEmotion = EmotionType.Neutral;
                    break;

                case NPCState.Fleeing:
                    if (_gestures != null) _gestures.PlayFear();
                    break;

                case NPCState.Hugging:
                    if (_gestures != null) _gestures.PlayAffection();
                    break;

                case NPCState.ChillingInGroup:
                    if (_gestures != null) _gestures.PlayJoy();
                    break;
            }
        }

        // ─── Coroutines ───────────────────────────────────────────────────

        private IEnumerator ReactRoutine()
        {
            currentState = NPCState.Reacting;
            yield return new WaitForSeconds(Random.Range(3f, 7f)); // variable react time
            if (currentState == NPCState.Reacting)
                EnterState(NPCState.WalkingHome);
        }

        // ─── External Events ──────────────────────────────────────────────

        public void OnPhaseChanged(DayPhase newPhase)
        {
            switch (newPhase)
            {
                case DayPhase.Morning:
                    _hasReadTodayNews = false;
                    if (_occupiedBench != null)
                    {
                        _occupiedBench.Vacate();
                        _occupiedBench = null;
                    }
                    if (_pendingNews != null && boardPosition != null)
                    {
                        _needs.HasPendingNews = true;
                        if (_actionPlanner != null) _actionPlanner.ForceReevaluate();
                    }
                    _needs.IsNightTime = false;
                    break;

                case DayPhase.Midday:
                    // Observational dynamics (pantomime / gossip) could go here
                    // but movement is handled purely by Utility AI.
                    break;

                case DayPhase.Afternoon:
                    // Finding a bench is now a Utility Action (TODO: SitAction)
                    break;

                case DayPhase.Night:
                    _needs.IsNightTime = true;
                    if (_actionPlanner != null) _actionPlanner.ForceReevaluate();
                    break;
            }
        }
        public void ReceiveNews(NewsPublishedData newsData)
        {
            _pendingNews      = newsData;
            _hasReadTodayNews = false;
        }

        // ─── News Processing ──────────────────────────────────────────────

        public void ProcessReadNews()
        {
            if (_pendingNews == null || _hasReadTodayNews) return;
            _hasReadTodayNews = true;

            var story = _pendingNews.FrontPage;
            if (story != null)
            {
                // Core emotional reaction to category
                if (personality != null)
                {
                    var reaction = personality.GetReactionTo(story.Category);
                    currentEmotion = reaction.Emotion;
                }
                else
                {
                    // Fallback emotional state
                    currentEmotion = story.Category == NewsCategory.Scandal || story.Category == NewsCategory.Disaster
                        ? EmotionType.Angry
                        : EmotionType.Happy;
                }

                // Photographed Sandbox Influence!
                if (story.SourcePhoto != null && story.SourcePhoto.PrimarySubject != null)
                {
                    var targetNpc = story.SourcePhoto.PrimarySubject.GetComponent<NPCController>();
                    if (targetNpc != null)
                    {
                        if (targetNpc.NpcId == NpcId)
                        {
                            // 📰 Case A: I am reading about MYSELF in the paper!
                            if (story.Category == NewsCategory.Scandal || story.Category == NewsCategory.Disaster)
                            {
                                currentEmotion = EmotionType.Angry;
                                TriggerReaction("💔", VisualUtils.Terracotta);
                                if (JournalManager.Instance != null)
                                {
                                    JournalManager.Instance.AddObservation($"[{NpcName}] [NEWS-SCANDAL] </3 :(", VisualUtils.Terracotta);
                                }
                            }
                            else
                            {
                                currentEmotion = EmotionType.Happy;
                                TriggerReaction("⭐", new Color(1f, 0.85f, 0.2f));
                                if (JournalManager.Instance != null)
                                {
                                    JournalManager.Instance.AddObservation($"[{NpcName}] [NEWS-HERO] [*] :)", new Color(1f, 0.85f, 0.2f));
                                }
                            }
                        }
                        else
                        {
                            // 📰 Case B: I am reading about someone else in the paper!
                            if (RelationshipMatrix.Instance != null)
                            {
                                if (story.Category == NewsCategory.Scandal || story.Category == NewsCategory.Disaster)
                                {
                                    // Scandals decrease reputation!
                                    RelationshipMatrix.Instance.ModifyRelationship(NpcId, targetNpc.NpcId, -12);
                                    TriggerReaction("[FLAME]", VisualUtils.Terracotta);
                                }
                                else
                                {
                                    // Heroics/Local stories increase reputation!
                                    RelationshipMatrix.Instance.ModifyRelationship(NpcId, targetNpc.NpcId, +10);
                                    TriggerReaction("[ALLY]", Color.cyan);
                                }
                            }
                        }
                    }
                }
            }

            _pendingNews = null;
        }

        // Visuals are now handled by NPCLocomotion.cs

        private void CreateLabel()
        {
            var labelGo = new GameObject("ID_Label");
            labelGo.transform.SetParent(transform);
            labelGo.transform.localPosition = new Vector3(0, 2.5f, 0);
            var t = labelGo.AddComponent<TextMesh>();
            t.text          = $"{NpcName} ({NpcId})";
            t.characterSize = 0.15f;
            t.anchor        = TextAnchor.MiddleCenter;
            t.alignment     = TextAlignment.Center;
            t.color         = Color.white;
            t.fontStyle     = FontStyle.Bold;
        }
        public void OnRelationshipUpdate()
        {
            // Observational dynamic shift: re-evaluate daily pathing
            Debug.Log($"[NPC_{NpcId}] Relationship matrix updated! Social links recalculated.");
        }

        public void TriggerReaction(string emoji, Color color)
        {
            if (GPOyun.UI.HUDManager.Instance != null)
            {
                GPOyun.UI.HUDManager.Instance.SpawnEmojiReaction(transform, emoji, color);
            }
        }

        private IEnumerator PlayerProximityCheckRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2.0f, 3.5f));

                if (currentState == NPCState.Sitting || currentState == NPCState.Reading || currentState == NPCState.Socializing || currentState == NPCState.Reacting || currentState == NPCState.Fleeing)
                    continue;

                // Find player in scene
                var player = FindAnyObjectByType<Player.PlayerController>();
                if (player == null) continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);

                if (dist < 5.0f)
                {
                    // Face player
                    Vector3 lookPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                    if (Vector3.Distance(transform.position, lookPos) > 0.1f)
                    {
                        transform.rotation = Quaternion.LookRotation(lookPos - transform.position);
                    }

                    if (relationshipWithPlayer >= 50)
                    {
                        // Friendly: wave and walk towards/follow player!
                        
                        currentState = NPCState.Wandering;
                        _targetPosition = player.transform.position + Random.insideUnitSphere * 1.5f;
                        _targetPosition.y = transform.position.y;
                        
                        TriggerReaction("🥰", new Color(1f, 0.4f, 0.6f));
                        yield return new WaitForSeconds(Random.Range(2.5f, 4.0f));
                    }
                    else if (relationshipWithPlayer <= -50)
                    {
                        // Hostile: flee away!
                        
                        _needs.IsPlayerHostile = true;
                        _needs.IsPlayerNearby = true;
                        if (_actionPlanner != null) _actionPlanner.ForceReevaluate();
                    }
                }
            }
        }

        public void OnPhotographedByPlayer()
        {
            // Face player
            var player = FindAnyObjectByType<Player.PlayerController>();
            if (player != null)
            {
                Vector3 lookPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                if (Vector3.Distance(transform.position, lookPos) > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(lookPos - transform.position);
                }
            }

            // Pose/freeze briefly
            currentState = NPCState.Reacting;
            
            if (relationshipWithPlayer >= 50)
            {
                relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + 15, -100, 100);
                TriggerReaction("🥰", new Color(1f, 0.4f, 0.6f));
            }
            else if (relationshipWithPlayer <= -50)
            {
                relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer - 10, -100, 100);
                TriggerReaction("🤬", VisualUtils.Terracotta);
            }
            else
            {
                relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + 10, -100, 100);
                TriggerReaction("⭐", new Color(1f, 0.85f, 0.2f));
            }

            StartCoroutine(ResumeAutonomyAfterPose(Random.Range(2.0f, 3.5f)));
        }

        private IEnumerator ResumeAutonomyAfterPose(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (currentState == NPCState.Reacting)
            {
                currentState = NPCState.Idle;
            }
        }
    }
}
