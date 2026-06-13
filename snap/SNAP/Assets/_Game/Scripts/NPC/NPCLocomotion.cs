using UnityEngine;
using UnityEngine.AI;

namespace GPOyun.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NPCController))]
    public class NPCLocomotion : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Animator _animator;
        private NPCController _controller;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _controller = GetComponent<NPCController>();

            // Configure agent and disable Root Motion which overrides NavMesh movement!
            if (_animator != null) _animator.applyRootMotion = false;
            
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.stoppingDistance = 0.5f;
            _agent.acceleration = 8f;

            // Force snap to NavMesh! If they spawn slightly off, they will never move!
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }

            // Disable Rigidbody physics to prevent NavMeshAgent conflict
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        private void Update()
        {
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (_animator == null || _controller == null) return;
            
            bool isWalking = _controller.currentState == NPCState.Wandering ||
                             _controller.currentState == NPCState.WalkingToBoard ||
                             _controller.currentState == NPCState.WalkingHome ||
                             _controller.currentState == NPCState.Fleeing ||
                             _controller.currentState == NPCState.Socializing ||
                             _controller.currentState == NPCState.Traveling;
            
            float targetSpeed = 0f;
            if (isWalking && _agent != null && !_agent.isStopped && _agent.velocity.sqrMagnitude > 0.01f || _agent.hasPath)
            {
                targetSpeed = _agent.speed;
            }
            
            _animator.SetFloat("Speed", targetSpeed);
            _animator.SetInteger("Emotion", (int)_controller.currentEmotion);
        }
    }
}
