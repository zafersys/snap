using UnityEngine;
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
        private Vector3  _targetPosition;
        private Animator _animator;
        private Coroutine _reactCoroutine;
        private Coroutine _wanderCoroutine;
        private NewsPublishedData _pendingNews;
        private bool _hasReadTodayNews;
        private bool _desiresLoneliness = false;
        private PantomimeGestures _gestures;
        private Environment.BenchObject _occupiedBench;
        private float _socializeCooldownTimer = 0f;
        private Coroutine _socializeCoroutine;
        private string _partnerEmoji = "";
        private Color _partnerEmojiColor = Color.white;

        // Neural Subsystems
        private Sensory.NPCSensoryMatrix _sensoryMatrix;
        private Appraisal.NPCAppraisalEngine _appraisalEngine;
        private Memory.NPCMemoryStream _memoryStream;

        // Ghost bob
        private float _bobTimer;
        public float bobFrequency = 1.5f;
        public float bobAmplitude = 0.08f;

        // Wander constraints (assigned by builder or detected)
        private static readonly float TOWN_RADIUS = 14f;

        // ─── Unity Events ──────────────────────────────────────────────────

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _homePosition = transform.position;
            
            _gestures = GetComponent<PantomimeGestures>();
            if (_gestures == null) _gestures = gameObject.AddComponent<PantomimeGestures>();

            // Setup Neural Subsystems
            _sensoryMatrix = gameObject.AddComponent<Sensory.NPCSensoryMatrix>();
            _appraisalEngine = gameObject.AddComponent<Appraisal.NPCAppraisalEngine>();
            _memoryStream = gameObject.AddComponent<Memory.NPCMemoryStream>();

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
                    if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                    currentState = NPCState.Reacting;
                    TriggerReaction("[ANGRY]", VisualUtils.Terracotta);
                    StartCoroutine(ResumeAutonomyAfterPose(3f));
                }
            }

            _memoryStream.AddMemory(new Data.MemoryEvent(Time.time, stimulus.Type, stimulus.SourceId, currentEmotion));
            relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + result.RelationshipDelta, -100, 100);
        }

        private void Start()
        {
            if (NPCManager.Instance != null) NPCManager.Instance.Register(this);
            // Stagger wander start so all NPCs don't move simultaneously
            float delay = Random.Range(0f, 3f);
            StartCoroutine(DelayedWanderStart(delay));
            
            if (_appraisalEngine != null) _appraisalEngine.Initialize(personality);
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
            HandleMovement();
            UpdateAnimation();
            UpdateBob();
        }

        // ─── Movement ─────────────────────────────────────────────────────

        private void HandleMovement()
        {
            if (currentState == NPCState.Wandering ||
                currentState == NPCState.WalkingToBoard ||
                currentState == NPCState.WalkingHome ||
                currentState == NPCState.Fleeing ||
                currentState == NPCState.Sitting)
            {
                float speed = currentState == NPCState.WalkingToBoard || currentState == NPCState.Fleeing ? runSpeed : moveSpeed;
                MoveTowards(_targetPosition, speed);

                float dist = Vector3.Distance(transform.position, _targetPosition);

                if (currentState == NPCState.WalkingToBoard && dist < 1.2f)
                {
                    EnterState(NPCState.Reading);
                }
                else if (currentState == NPCState.WalkingHome && dist < 0.8f)
                {
                    transform.position = new Vector3(_homePosition.x, transform.position.y, _homePosition.z);
                    EnterState(NPCState.Idle);
                    _wanderCoroutine = StartCoroutine(WanderRoutine());
                }
                else if (currentState == NPCState.Wandering && dist < 0.5f)
                {
                    EnterState(NPCState.Idle);
                    _wanderCoroutine = StartCoroutine(WanderRoutine());
                }
                else if (currentState == NPCState.Fleeing && dist < 1.0f)
                {
                    EnterState(NPCState.Idle);
                    _wanderCoroutine = StartCoroutine(WanderRoutine());
                }
                else if (currentState == NPCState.Sitting && dist < 0.5f)
                {
                    transform.position = _targetPosition;
                    // Sit procedural compress
                    if (_gestures != null) _gestures.SetSadness(false); // Clear sadness squashes
                    transform.localScale = new Vector3(transform.localScale.x, 0.7f, transform.localScale.z);
                }
            }
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            Vector3 flat = new Vector3(target.x, transform.position.y, target.z);
            Vector3 dir  = (flat - transform.position).normalized;

            if (dir.sqrMagnitude < 0.01f) return;

            // Obstacle avoidance
            var avoidance = GetComponent<Core.ObstacleAvoidance>();
            if (avoidance != null)
            {
                dir = (dir + avoidance.GetAvoidanceVector(dir)).normalized;
            }

            // Smooth rotation
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

            // Move
            transform.position += dir * speed * Time.deltaTime;
        }

        // ─── State Machine ────────────────────────────────────────────────

        private void EnterState(NPCState newState)
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

        private IEnumerator DelayedWanderStart(float delay)
        {
            yield return new WaitForSeconds(delay);
            _wanderCoroutine = StartCoroutine(WanderRoutine());
        }

        /// <summary>
        /// Main autonomy loop. NPC idles, then picks a random destination
        /// nearby and walks to it. Repeats until interrupted by phase events.
        /// </summary>
        private IEnumerator WanderRoutine()
        {
            while (true)
            {
                // Idle pause
                float idleTime = Random.Range(minIdleTime, maxIdleTime);
                currentState = NPCState.Idle;
                yield return new WaitForSeconds(idleTime);

                // If interrupted by an external state change, stop
                if (currentState != NPCState.Idle) yield break;

                // 20% chance to desire some quiet lonely chill-time!
                if (Random.value < 0.20f && !_desiresLoneliness)
                {
                    yield return StartCoroutine(ChillAloneRoutine());
                    continue;
                }

                // 15% chance to love walking and travel outside the village!
                if (Random.value < 0.15f && !_desiresLoneliness)
                {
                    yield return StartCoroutine(TravelOutsideVillageRoutine());
                    continue;
                }

                // Pick wander target
                Vector3 dest = PickWanderDestination();
                _targetPosition = dest;
                currentState = NPCState.Wandering;

                // Walk until we arrive (HandleMovement finishes this)
                yield return StartCoroutine(WaitForArrival(NPCState.Wandering, 0.5f));

                if (currentState != NPCState.Wandering) yield break;
                // Arrived — loop back to idle
            }
        }

        private IEnumerator TravelOutsideVillageRoutine()
        {
            currentState = NPCState.Traveling;
            _socializeCooldownTimer = 35f; // suppress socialization while traveling

            // Pick a scenic location way outside the village boundaries (radial distance of 24-34m)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float travelDistance = Random.Range(24f, 34f);
            Vector3 outsideTarget = new Vector3(Mathf.Cos(angle) * travelDistance, transform.position.y, Mathf.Sin(angle) * travelDistance);
            _targetPosition = outsideTarget;

            // Travel reaction and observation logging!
            TriggerReaction("[TRAVEL]", new Color(0.4f, 0.8f, 0.4f));

            if (JournalManager.Instance != null && Random.value < 0.4f)
            {
                JournalManager.Instance.AddObservation($"[{NpcName}] [TRAVEL] -> [OUTSIDE-VILLAGE] \\o/", new Color(0.4f, 0.8f, 0.4f));
            }

            // Walk happily to the target
            yield return StartCoroutine(WaitForArrival(NPCState.Traveling, 1.2f, 20f));

            if (currentState != NPCState.Traveling) yield break;

            // Arrived! Stay at the viewpoint and admire the countryside landscape
            currentState = NPCState.Idle;
            yield return new WaitForSeconds(Random.Range(10f, 18f));

            // Return back to the town square happily
            currentState = NPCState.Wandering;
            _targetPosition = PickWanderDestination();

            yield return StartCoroutine(WaitForArrival(NPCState.Wandering, 1.0f));

            currentState = NPCState.Idle;
        }

        private IEnumerator ChillAloneRoutine()
        {
            _desiresLoneliness = true;
            _socializeCooldownTimer = 25f; // suppress instant socialization

            // Pick a scenic, quiet spot on the outer edge of town
            Vector3 scenicSpot = PickWanderDestination() * 1.3f;
            scenicSpot.y = transform.position.y;
            _targetPosition = scenicSpot;

            // Walk to the peaceful scenic spot
            currentState = NPCState.Wandering;
            yield return StartCoroutine(WaitForArrival(NPCState.Wandering, 0.8f));

            // Enjoy the silence and cozy landscape
            currentState = NPCState.Sitting;
            TriggerReaction("[CHILL]", new Color(0.6f, 0.9f, 0.9f));

            if (JournalManager.Instance != null && Random.value < 0.35f)
            {
                JournalManager.Instance.AddObservation($"[{NpcName}] [CHILL] [ALONE] ~_~", new Color(0.6f, 0.9f, 0.9f));
            }

            // Chill peacefully for 15-28 seconds!
            yield return new WaitForSeconds(Random.Range(15f, 28f));

            _desiresLoneliness = false;
            currentState = NPCState.Idle;
        }

        private IEnumerator WaitForArrival(NPCState expectedState, float tolerance, float maxTimeout = 12f)
        {
            float elapsed = 0f;
            Vector3 lastPos = transform.position;
            float stuckTimer = 0f;

            while (currentState == expectedState && Vector3.Distance(transform.position, _targetPosition) > tolerance && elapsed < maxTimeout)
            {
                elapsed += Time.deltaTime;
                
                if (Vector3.Distance(transform.position, lastPos) < 0.05f)
                {
                    stuckTimer += Time.deltaTime;
                    if (stuckTimer > 1.5f)
                    {
                        Debug.LogWarning($"[NPCController] {NpcName} is STUCK while in {expectedState}! Recovering...");
                        Vector3 jitter = Random.insideUnitSphere * 0.6f;
                        jitter.y = 0;
                        transform.position += jitter;
                        stuckTimer = 0f;
                    }
                }
                else
                {
                    stuckTimer = 0f;
                    lastPos = transform.position;
                }
                
                yield return null;
            }
        }

        /// <summary>Picks a random walkable point around the town center.</summary>
        private Vector3 PickWanderDestination()
        {
            for (int attempt = 0; attempt < 10; attempt++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Random.Range(minWanderDist, maxWanderDist);
                Vector3 candidate = new Vector3(
                    Mathf.Cos(angle) * dist,
                    transform.position.y,
                    Mathf.Sin(angle) * dist
                );

                // Keep inside town radius
                if (candidate.magnitude <= TOWN_RADIUS)
                    return candidate;
            }

            // Fallback: walk slightly toward centre
            return Vector3.Lerp(transform.position, Vector3.zero, 0.3f);
        }

        private IEnumerator ReactRoutine()
        {
            currentState = NPCState.Reacting;
            yield return new WaitForSeconds(Random.Range(3f, 7f)); // variable react time
            if (currentState == NPCState.Reacting)
                EnterState(NPCState.WalkingHome);
        }

        private IEnumerator SocialProximityScanRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1.2f, 2.2f)); // Scan periodically

                if (_desiresLoneliness) continue;
                if (currentState != NPCState.Wandering && currentState != NPCState.Idle) continue;
                if (_socializeCooldownTimer > 0f) continue;

                if (NPCManager.Instance == null || RelationshipMatrix.Instance == null) continue;

                NPCController partner = null;
                float bestDist = 3.2f;

                foreach (var other in NPCManager.Instance.GetAll())
                {
                    if (other == this) continue;
                    if (other._desiresLoneliness) continue;
                    if (other.currentState != NPCState.Wandering && other.currentState != NPCState.Idle) continue;
                    if (other._socializeCooldownTimer > 0f) continue;

                    float dist = Vector3.Distance(transform.position, other.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        partner = other;
                    }
                }

                if (partner != null)
                {
                    StartSocializing(partner);
                }
            }
        }

        private void StartSocializing(NPCController partner)
        {
            if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
            if (partner._wanderCoroutine != null) partner.StopCoroutine(partner._wanderCoroutine);

            EnterState(NPCState.Socializing);
            partner.EnterState(NPCState.Socializing);

            if (_socializeCoroutine != null) StopCoroutine(_socializeCoroutine);
            _socializeCoroutine = StartCoroutine(SocializeRoutine(partner, true));

            if (partner._socializeCoroutine != null) partner.StopCoroutine(partner._socializeCoroutine);
            partner._socializeCoroutine = partner.StartCoroutine(partner.SocializeRoutine(this, false));
        }

        private IEnumerator SocializeRoutine(NPCController partner, bool isInitiator)
        {
            // Face the partner
            Vector3 lookPos = new Vector3(partner.transform.position.x, transform.position.y, partner.transform.position.z);
            if (Vector3.Distance(transform.position, lookPos) > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(lookPos - transform.position);
            }

            int rel = RelationshipMatrix.Instance != null ? RelationshipMatrix.Instance.GetRelationship(NpcId, partner.NpcId) : 0;
            int delta = 0;
            string emoji = "[TALK]";
            Color emojiColor = Color.white;

            if (isInitiator)
            {
                // Reset partner syncs
                partner._partnerEmoji = "";

                // Roll one of the 10 dynamic dimensions!
                int dimension = Random.Range(1, 11);
                switch (dimension)
                {
                    case 1: // Flame (Fiery Rivalry)
                        delta = -18;
                        emoji = "[FLAME]";
                        emojiColor = VisualUtils.Terracotta;
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [FLAME] >:( [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 2: // Star (Admiration / Spark)
                        delta = 15;
                        emoji = "[STAR]";
                        emojiColor = new Color(1f, 0.85f, 0.2f);
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [STAR] <3 [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 3: // Gift (Acts of Kindness)
                        delta = 12;
                        emoji = "[GIFT]";
                        emojiColor = new Color(0.95f, 0.35f, 0.95f);
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [GIFT] :) [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 4: // Pizza (Sharing food)
                        delta = 10;
                        emoji = "[PIZZA]";
                        emojiColor = new Color(1f, 0.6f, 0.2f);
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [PIZZA] :D [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 5: // Zzz (Boredom Walkaway)
                        delta = 0;
                        emoji = "[ZZZ]";
                        emojiColor = Color.gray;
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [ZZZ] ~_~ [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 6: // Shush (Secret Gossip spreading)
                        delta = 3;
                        emoji = "[SHH]";
                        emojiColor = Color.yellow;
                        int thirdId = NpcId;
                        string thirdName = NpcName;
                        if (NPCManager.Instance != null)
                        {
                            var allNpcs = NPCManager.Instance.GetAll();
                            if (allNpcs.Count > 2)
                            {
                                int idx = Random.Range(0, allNpcs.Count);
                                while (allNpcs[idx] == this || allNpcs[idx] == partner)
                                {
                                    idx = Random.Range(0, allNpcs.Count);
                                }
                                thirdId = allNpcs[idx].NpcId;
                                thirdName = allNpcs[idx].NpcName;
                                int gossipDelta = Random.value < 0.5f ? 10 : -10;
                                if (RelationshipMatrix.Instance != null)
                                {
                                    RelationshipMatrix.Instance.ModifyRelationship(partner.NpcId, thirdId, gossipDelta);
                                }
                            }
                        }
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [SHH] [{partner.NpcName}] -> [{thirdName}]", emojiColor);
                        }
                        break;
                    case 7: // Drama (Teasing)
                        delta = Random.Range(-15, 16);
                        emoji = "[DRAMA]";
                        emojiColor = new Color(0.6f, 0.3f, 0.9f);
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [DRAMA] ?_? [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 8: // Celebration (Dancing)
                        delta = 6;
                        emoji = "[CELEB]";
                        emojiColor = new Color(1f, 0.4f, 0.4f);
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [CELEB] \\o/ [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 9: // Broken Heart (Betrayal)
                        delta = -25;
                        emoji = "[BROKEN]";
                        emojiColor = Color.red;
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [BROKEN] </3 [{partner.NpcName}]", emojiColor);
                        }
                        break;
                    case 10: // Handshake (Cozy Alliance)
                        delta = 8;
                        emoji = "[ALLY]";
                        emojiColor = Color.cyan;
                        if (JournalManager.Instance != null)
                        {
                            JournalManager.Instance.AddObservation($"[{NpcName}] [ALLY] [{partner.NpcName}]", emojiColor);
                        }
                        break;
                }

                if (RelationshipMatrix.Instance != null)
                {
                    RelationshipMatrix.Instance.ModifyRelationship(NpcId, partner.NpcId, delta);
                }

                // Send sync emoji to partner
                partner._partnerEmoji = emoji;
                partner._partnerEmojiColor = emojiColor;
            }
            else
            {
                // Responder: wait briefly for synchronization
                float waitTime = 0f;
                while (string.IsNullOrEmpty(_partnerEmoji) && waitTime < 0.5f)
                {
                    yield return new WaitForSeconds(0.05f);
                    waitTime += 0.05f;
                }

                if (!string.IsNullOrEmpty(_partnerEmoji))
                {
                    emoji = _partnerEmoji;
                    emojiColor = _partnerEmojiColor;
                }
            }

            // Sync physical gestures to dynamic emoji
            if (emoji == "[FLAME]" || emoji == "[BROKEN]")
            {
                if (_gestures != null) _gestures.PlayFear();
            }
            else if (emoji == "[STAR]" || emoji == "[GIFT]" || emoji == "[ALLY]")
            {
                if (_gestures != null) _gestures.PlayAffection();
            }
            else
            {
                if (_gestures != null) _gestures.PlayJoy();
            }

            TriggerReaction(emoji, emojiColor);

            yield return new WaitForSeconds(Random.Range(4.0f, 5.5f));

            if (emoji == "[PIZZA]")
            {
                _socializeCooldownTimer = 10.0f; // longer pizza hangout
            }
            else if (emoji == "[ZZZ]")
            {
                _socializeCooldownTimer = 35.0f; // long snooze cooldown
            }
            else
            {
                _socializeCooldownTimer = 18.0f;
            }

            // --- Walk together / Flee away mechanics! ---
            int finalRel = RelationshipMatrix.Instance != null ? RelationshipMatrix.Instance.GetRelationship(NpcId, partner.NpcId) : 0;
            if (isInitiator)
            {
                if (finalRel >= 50)
                {
                    // Friends: Walk together to a shared destination!
                    Vector3 dest = PickWanderDestination();
                    
                    if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                    _targetPosition = dest;
                    _wanderCoroutine = StartCoroutine(WalkTogetherRoutine(dest));

                    if (partner._wanderCoroutine != null) partner.StopCoroutine(partner._wanderCoroutine);
                    partner._targetPosition = dest + new Vector3(Random.Range(-0.8f, 0.8f), 0f, Random.Range(-0.8f, 0.8f));
                    partner._wanderCoroutine = partner.StartCoroutine(partner.WalkTogetherRoutine(partner._targetPosition));
                }
                else if (finalRel <= -50)
                {
                    // Rivals: Flee away in opposite directions immediately!
                    Vector3 dirAway = (transform.position - partner.transform.position).normalized;
                    Vector3 myDest = transform.position + dirAway * 8f;
                    Vector3 partnerDest = partner.transform.position - dirAway * 8f;

                    if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                    _targetPosition = myDest;
                    _wanderCoroutine = StartCoroutine(WalkTogetherRoutine(myDest));

                    if (partner._wanderCoroutine != null) partner.StopCoroutine(partner._wanderCoroutine);
                    partner._targetPosition = partnerDest;
                    partner._wanderCoroutine = partner.StartCoroutine(partner.WalkTogetherRoutine(partnerDest));
                }
                else
                {
                    // Neutrals: enter autonomy normally
                    EnterState(NPCState.Idle);
                    _wanderCoroutine = StartCoroutine(WanderRoutine());

                    partner.EnterState(NPCState.Idle);
                    partner._wanderCoroutine = partner.StartCoroutine(partner.WanderRoutine());
                }
            }
        }

        private IEnumerator WalkTogetherRoutine(Vector3 dest)
        {
            currentState = NPCState.Wandering;
            _targetPosition = dest;
            yield return StartCoroutine(WaitForArrival(NPCState.Wandering, 0.8f));
            if (currentState == NPCState.Wandering)
            {
                EnterState(NPCState.Idle);
                _wanderCoroutine = StartCoroutine(WanderRoutine());
            }
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
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        float delay = Random.Range(0f, 8f);
                        StartCoroutine(GoToBoardDelayed(delay));
                    }
                    break;

                case DayPhase.Midday:
                    // OBSERVATIONAL RELATION DYNAMICS:
                    // Find closest friend or rival to drive silent pantomime behaviors
                    NPCController closestFriend = null;
                    NPCController closestRival = null;
                    float friendDist = 999f;
                    float rivalDist = 999f;

                    if (NPCManager.Instance != null && RelationshipMatrix.Instance != null)
                    {
                        foreach (var other in NPCManager.Instance.GetAll())
                        {
                            if (other == this) continue;
                            int rel = RelationshipMatrix.Instance.GetRelationship(NpcId, other.NpcId);
                            float dist = Vector3.Distance(transform.position, other.transform.position);

                            if (rel >= 50 && dist < friendDist)
                            {
                                friendDist = dist;
                                closestFriend = other;
                            }
                            else if (rel <= -50 && dist < rivalDist)
                            {
                                rivalDist = dist;
                                closestRival = other;
                            }
                        }
                    }

                    if (closestRival != null && rivalDist < 4.5f)
                    {
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        // Run away from rival
                        Vector3 dirAway = (transform.position - closestRival.transform.position).normalized;
                        _targetPosition = transform.position + dirAway * 7f;
                        EnterState(NPCState.Fleeing);
                    }
                    else if (closestFriend != null && friendDist < 6.0f)
                    {
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        _targetPosition = closestFriend.transform.position;
                        
                        if (friendDist < 1.6f)
                        {
                            // Close enough for hugs!
                            EnterState(NPCState.Hugging);
                            if (JournalManager.Instance != null)
                            {
                                JournalManager.Instance.AddObservation(
                                    $"[NPC_{NpcId}] [HUG] <3 [NPC_{closestFriend.NpcId}]",
                                    Color.yellow
                                );
                            }
                        }
                        else
                        {
                            // Walk towards friend
                            currentState = NPCState.Wandering;
                        }
                    }
                    else
                    {
                        if (currentState == NPCState.Idle || currentState == NPCState.Wandering)
                        {
                            if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                            wanderRadius = 5f;
                            _wanderCoroutine = StartCoroutine(WanderRoutine());
                        }
                    }
                    break;

                case DayPhase.Afternoon:
                    // Search active benches to sit down and relax
                    var benches = FindObjectsByType<Environment.BenchObject>();
                    Environment.BenchObject freeBench = null;
                    foreach (var b in benches)
                    {
                        if (!b.IsOccupied)
                        {
                            freeBench = b;
                            break;
                        }
                    }

                    if (freeBench != null && freeBench.TryOccupy(this))
                    {
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        _occupiedBench = freeBench;
                        _targetPosition = freeBench.GetSitPosition();
                        EnterState(NPCState.Sitting);
                    }
                    else
                    {
                        wanderRadius = 10f;
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        _wanderCoroutine = StartCoroutine(WanderRoutine());
                    }
                    break;

                case DayPhase.Night:
                    if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                    if (_reactCoroutine != null) StopCoroutine(_reactCoroutine);
                    EnterState(NPCState.WalkingHome);
                    break;
            }
        }

        private IEnumerator GoToBoardDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (!_hasReadTodayNews && boardPosition != null)
            {
                _targetPosition = boardPosition.position;
                currentState    = NPCState.WalkingToBoard;
            }
        }

        public void ReceiveNews(NewsPublishedData newsData)
        {
            _pendingNews      = newsData;
            _hasReadTodayNews = false;
        }

        // ─── News Processing ──────────────────────────────────────────────

        private void ProcessReadNews()
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
                                TriggerReaction("[BROKEN]", VisualUtils.Terracotta);
                                if (JournalManager.Instance != null)
                                {
                                    JournalManager.Instance.AddObservation($"[{NpcName}] [NEWS-SCANDAL] </3 :(", VisualUtils.Terracotta);
                                }
                            }
                            else
                            {
                                currentEmotion = EmotionType.Happy;
                                TriggerReaction("[STAR]", new Color(1f, 0.85f, 0.2f));
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

        // ─── Visuals ──────────────────────────────────────────────────────

        private void UpdateAnimation()
        {
            if (_animator == null) return;
            bool isWalking = currentState == NPCState.Wandering ||
                             currentState == NPCState.WalkingToBoard ||
                             currentState == NPCState.WalkingHome;
            _animator.SetFloat("Speed",   isWalking ? moveSpeed : 0f);
            _animator.SetInteger("Emotion", (int)currentEmotion);
        }

        private void UpdateBob()
        {
            _bobTimer += Time.deltaTime;
            float bob = Mathf.Sin(_bobTimer * bobFrequency) * bobAmplitude;

            Transform bodyT = transform.Find("Body");
            Transform headT = transform.Find("Head");
            if (bodyT != null) bodyT.localPosition = new Vector3(0, bob, 0);
            if (headT != null) headT.localPosition  = new Vector3(0, 1.2f + bob, 0);
        }

        private void CreateLabel()
        {
            var labelGo = new GameObject("ID_Label");
            labelGo.transform.SetParent(transform);
            labelGo.transform.localPosition = new Vector3(0, 2.5f, 0);
            var t = labelGo.AddComponent<TextMesh>();
            t.text          = $"NPC {NpcId}";
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
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        
                        currentState = NPCState.Wandering;
                        _targetPosition = player.transform.position + Random.insideUnitSphere * 1.5f;
                        _targetPosition.y = transform.position.y;
                        
                        TriggerReaction("[HAPPY]", new Color(1f, 0.4f, 0.6f));
                        yield return new WaitForSeconds(Random.Range(2.5f, 4.0f));
                    }
                    else if (relationshipWithPlayer <= -50)
                    {
                        // Hostile: flee away!
                        if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
                        
                        Vector3 dirAway = (transform.position - player.transform.position).normalized;
                        _targetPosition = transform.position + dirAway * 8f;
                        _targetPosition.y = transform.position.y;
                        
                        currentState = NPCState.Fleeing;
                        TriggerReaction("[ANGRY]", VisualUtils.Terracotta);
                        yield return StartCoroutine(WaitForArrival(NPCState.Fleeing, 1.0f));
                        
                        currentState = NPCState.Idle;
                        _wanderCoroutine = StartCoroutine(WanderRoutine());
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
            if (_wanderCoroutine != null) StopCoroutine(_wanderCoroutine);
            currentState = NPCState.Reacting;
            
            if (relationshipWithPlayer >= 50)
            {
                relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + 15, -100, 100);
                TriggerReaction("[HAPPY]", new Color(1f, 0.4f, 0.6f));
            }
            else if (relationshipWithPlayer <= -50)
            {
                relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer - 10, -100, 100);
                TriggerReaction("[ANGRY]", VisualUtils.Terracotta);
            }
            else
            {
                relationshipWithPlayer = Mathf.Clamp(relationshipWithPlayer + 10, -100, 100);
                TriggerReaction("[STAR]", new Color(1f, 0.85f, 0.2f));
            }

            StartCoroutine(ResumeAutonomyAfterPose(Random.Range(2.0f, 3.5f)));
        }

        private IEnumerator ResumeAutonomyAfterPose(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (currentState == NPCState.Reacting)
            {
                currentState = NPCState.Idle;
                _wanderCoroutine = StartCoroutine(WanderRoutine());
            }
        }
    }
}
