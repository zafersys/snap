using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GPOyun.NPC.UtilityAI
{
    public abstract class MoveAction : NPCAction
    {
        protected NavMeshAgent _agent;
        protected bool _isExecuting = false;

        public override void Initialize(NPCController controller, NPCNeeds needs)
        {
            base.Initialize(controller, needs);
            _agent = controller.GetComponent<NavMeshAgent>();
        }

        protected IEnumerator MoveToTarget(Vector3 targetPosition, NPCState walkingState = NPCState.Wandering, float arrivalDistance = 1.2f, float maxStuckTime = 5f)
        {
            Controller.EnterState(walkingState);

            bool useNavMesh = (_agent != null && _agent.isOnNavMesh);

            if (useNavMesh)
            {
                // Ensure destination is valid on NavMesh
                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                {
                    _agent.SetDestination(hit.position);
                    _agent.speed = Controller.moveSpeed;
                    _agent.isStopped = false;

                    float stuckTimer = 0f;
                    while (_isExecuting && (_agent.pathPending || _agent.remainingDistance > arrivalDistance))
                    {
                        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid || stuckTimer > maxStuckTime)
                        {
                            break;
                        }

                        if (!_agent.pathPending && _agent.velocity.sqrMagnitude < 0.1f)
                        {
                            stuckTimer += Time.deltaTime;
                        }
                        else
                        {
                            stuckTimer = 0f;
                        }

                        yield return null;
                    }

                    if (_agent != null && _agent.isOnNavMesh)
                    {
                        _agent.isStopped = true;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                // FALLBACK: Brute-force movement ignoring NavMesh physics!
                // This guarantees the character moves even if the NavMesh is completely missing or broken.
                float timeout = 10f;
                float timer = 0f;
                while (_isExecuting && Vector3.Distance(transform.position, targetPosition) > arrivalDistance)
                {
                    // Prevent flying into the sky or underground!
                    targetPosition.y = transform.position.y;

                    // Rotate towards target
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * Controller.turnSpeed);
                    }

                    // Move towards target
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, Controller.moveSpeed * Time.deltaTime);
                    
                    // Simple animation trigger (handled in Locomotion usually, but we force it here if agent is dead)
                    Animator anim = GetComponent<Animator>();
                    if (anim != null) anim.SetFloat("Speed", Controller.moveSpeed);

                    timer += Time.deltaTime;
                    if (timer > timeout) break; // Don't walk forever into a wall

                    yield return null;
                }
            }
        }

        public override void Interrupt()
        {
            _isExecuting = false;
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
            }
        }
    }
}
