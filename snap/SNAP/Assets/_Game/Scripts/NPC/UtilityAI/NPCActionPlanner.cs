using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPOyun.NPC.UtilityAI
{
    public class NPCActionPlanner : MonoBehaviour
    {
        private NPCController _controller;
        private NPCNeeds _needs;
        
        private List<NPCAction> _availableActions = new List<NPCAction>();
        private NPCAction _activeAction;

        private float _evaluationTimer = 0f;
        private float _evaluationInterval = 1.0f; // Re-evaluate every 1 second

        public void Initialize(NPCController controller, NPCNeeds needs)
        {
            _controller = controller;
            _needs = needs;

            // Load all attached actions
            var actions = GetComponents<NPCAction>();
            foreach (var action in actions)
            {
                action.Initialize(controller, needs);
                _availableActions.Add(action);
            }
        }

        private void Start()
        {
            // Give each NPC a random start offset so their FSMs don't evaluate on the exact same frame!
            _evaluationTimer = Random.Range(0f, _evaluationInterval);
        }

        private void Update()
        {
            if (_controller == null) return;

            _evaluationTimer += Time.deltaTime;
            if (_evaluationTimer >= _evaluationInterval)
            {
                _evaluationTimer = 0f;
                EvaluateUtilities();
            }
        }

        public void ForceReevaluate()
        {
            _evaluationTimer = 0f;
            EvaluateUtilities();
        }

        public void ForceAction(System.Type actionType)
        {
            foreach (var action in _availableActions)
            {
                if (action.GetType() == actionType)
                {
                    if (_activeAction != action)
                    {
                        if (_activeAction != null)
                        {
                            _activeAction.Interrupt();
                            StopAllCoroutines();
                        }
                        
                        _activeAction = action;
                        StartCoroutine(_activeAction.Execute());
                        Debug.Log($"[NPC {_controller.NpcName}] FORCED Action: {_activeAction.ActionName}");
                    }
                    return;
                }
            }
        }

        private void EvaluateUtilities()
        {
            if (_availableActions.Count == 0) return;

            NPCAction bestAction = null;
            float highestUtility = -1f;

            foreach (var action in _availableActions)
            {
                float utility = action.CalculateUtility();
                if (utility > highestUtility)
                {
                    highestUtility = utility;
                    bestAction = action;
                }
            }

            // If a new action is vastly superior (or our current action is null), switch!
            // We add a tiny buffer (e.g. 5 points) to prevent rapid oscillating between two actions of similar utility.
            if (_activeAction == null || (bestAction != null && bestAction != _activeAction && highestUtility > (_activeAction.CalculateUtility() + 5f)))
            {
                if (_activeAction != null)
                {
                    Debug.Log($"[UtilityAI] {_controller.NpcName} interrupting {_activeAction.ActionName} for {bestAction.ActionName} (Utility: {highestUtility:F1})");
                    _activeAction.Interrupt();
                    StopAllCoroutines();
                }
                else
                {
                    Debug.Log($"[UtilityAI] {_controller.NpcName} starting FIRST action: {bestAction.ActionName} (Utility: {highestUtility:F1})");
                }

                _activeAction = bestAction;
                StartCoroutine(_activeAction.Execute());
            }
        }
    }
}
