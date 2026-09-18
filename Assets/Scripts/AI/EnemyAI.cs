using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HorrorTemplate.Audio;
using HorrorTemplate.Gameplay;

namespace HorrorTemplate.AI
{
    public enum EnemyState
    {
        Patrol,
        Investigate,
        Chase
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Patrol;

        [Header("Movement Speeds")]
        [Tooltip("Agent speed during routine patrolling.")]
        [SerializeField] private float patrolSpeed = 2.0f;
        
        [Tooltip("Agent speed when heading to investigate a suspicious noise.")]
        [SerializeField] private float investigateSpeed = 2.8f;
        
        [Tooltip("Agent speed when actively pursuing the player.")]
        [SerializeField] private float chaseSpeed = 4.5f;

        [Header("Patrol Settings")]
        [Tooltip("Array of waypoints the enemy will cycle through during patrol.")]
        [SerializeField] private Transform[] patrolPoints;
        
        [Tooltip("How many seconds the enemy waits at each waypoint before moving to the next.")]
        [SerializeField] private float waypointWaitTime = 3.0f;

        [Header("Investigation Settings")]
        [Tooltip("How many seconds the enemy searches at the noise location before resuming patrol.")]
        [SerializeField] private float investigateWaitTime = 4.0f;

        [Header("Vision Settings")]
        [Tooltip("The origin point from which sight rays are cast (e.g. the head/eyes). If null, defaults to body position + height offset.")]
        [SerializeField] private Transform eyePoint;
        
        [Tooltip("Maximum distance the enemy can see the player.")]
        [SerializeField] private float viewDistance = 14.0f;
        
        [Tooltip("Field of View angle in degrees.")]
        [SerializeField] private float viewAngle = 110.0f;
        
        [Tooltip("LayerMask containing physical walls, doors, and obstacles that block line of sight.")]
        [SerializeField] private LayerMask obstacleMask = ~0;
        
        [Tooltip("How many seconds the enemy continues chasing towards the last known position after losing sight of the player.")]
        [SerializeField] private float lostSightCooldown = 3.5f;

        [Header("Attack Settings")]
        [Tooltip("Distance at which the enemy catches the player.")]
        [SerializeField] private float catchDistance = 1.5f;

        // References & State Variables
        private NavMeshAgent agent;
        private Transform playerTransform;
        private int currentPatrolIndex;
        private float stateTimer;
        private Vector3 targetInvestigatePosition;
        private Vector3 lastKnownPlayerPosition;
        private float lostSightTimer;
        private bool isWaitingAtPoint;

        public EnemyState CurrentState => currentState;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            // Locate player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }

            SetState(EnemyState.Patrol);
        }

        private void OnEnable()
        {
            PlayerSoundEmitter.OnSoundEmitted += HandleSoundHeard;
        }

        private void OnDisable()
        {
            PlayerSoundEmitter.OnSoundEmitted -= HandleSoundHeard;
        }

        private void Update()
        {
            CheckVision();

            switch (currentState)
            {
                case EnemyState.Patrol:
                    UpdatePatrolState();
                    break;
                case EnemyState.Investigate:
                    UpdateInvestigateState();
                    break;
                case EnemyState.Chase:
                    UpdateChaseState();
                    break;
            }
        }

        #region Vision Logic

        private void CheckVision()
        {
            if (playerTransform == null) return;

            if (HasLineOfSightToPlayer())
            {
                lastKnownPlayerPosition = playerTransform.position;
                lostSightTimer = 0f;

                if (currentState != EnemyState.Chase)
                {
                    SetState(EnemyState.Chase);
                }
            }
        }

        private bool HasLineOfSightToPlayer()
        {
            if (playerTransform == null) return false;

            Vector3 eyePos = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;
            Vector3 targetCenter = playerTransform.position + Vector3.up * 1.0f;
            Vector3 dirToPlayer = (targetCenter - eyePos);
            float distToPlayer = dirToPlayer.magnitude;

            // Distance check
            if (distToPlayer > viewDistance)
            {
                return false;
            }

            // Angle check (Field of View)
            Vector3 forwardDir = eyePoint != null ? eyePoint.forward : transform.forward;
            float angle = Vector3.Angle(forwardDir, dirToPlayer.normalized);
            if (angle > viewAngle * 0.5f)
            {
                return false;
            }

            // Raycast obstacle check
            if (Physics.Raycast(eyePos, dirToPlayer.normalized, out RaycastHit hit, distToPlayer, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.root == playerTransform.root)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region State Machine

        public void SetState(EnemyState newState)
        {
            currentState = newState;
            stateTimer = 0f;
            isWaitingAtPoint = false;

            switch (currentState)
            {
                case EnemyState.Patrol:
                    agent.speed = patrolSpeed;
                    MoveToNextPatrolPoint();
                    break;

                case EnemyState.Investigate:
                    agent.speed = investigateSpeed;
                    agent.SetDestination(targetInvestigatePosition);
                    break;

                case EnemyState.Chase:
                    agent.speed = chaseSpeed;
                    agent.SetDestination(playerTransform.position);
                    break;
            }
        }

        private void UpdatePatrolState()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            // Check if agent reached waypoint
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                if (!isWaitingAtPoint)
                {
                    isWaitingAtPoint = true;
                    stateTimer = 0f;
                }

                stateTimer += Time.deltaTime;
                if (stateTimer >= waypointWaitTime)
                {
                    isWaitingAtPoint = false;
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    MoveToNextPatrolPoint();
                }
            }
        }

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            Transform targetPoint = patrolPoints[currentPatrolIndex];
            if (targetPoint != null)
            {
                agent.SetDestination(targetPoint.position);
            }
        }

        private void UpdateInvestigateState()
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                stateTimer += Time.deltaTime;
                if (stateTimer >= investigateWaitTime)
                {
                    // Finished searching, return to patrol
                    SetState(EnemyState.Patrol);
                }
            }
        }

        private void UpdateChaseState()
        {
            if (playerTransform == null)
            {
                SetState(EnemyState.Patrol);
                return;
            }

            if (Vector3.Distance(transform.position, playerTransform.position) <= catchDistance)
            {
                PlayerCaptureManager captureManager = playerTransform.GetComponent<PlayerCaptureManager>();
                if (captureManager != null)
                {
                    captureManager.CapturePlayer();
                    SetState(EnemyState.Patrol);
                    return;
                }
            }

            if (HasLineOfSightToPlayer())
            {
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                // Lost sight: head to last known position and count down cooldown
                agent.SetDestination(lastKnownPlayerPosition);
                lostSightTimer += Time.deltaTime;

                if (lostSightTimer >= lostSightCooldown)
                {
                    targetInvestigatePosition = lastKnownPlayerPosition;
                    SetState(EnemyState.Investigate);
                }
            }
        }

        #endregion

        #region Hearing Logic

        private void HandleSoundHeard(Vector3 soundPosition, float noiseRadius)
        {
            // If actively chasing with clear vision, ignore distractions
            if (currentState == EnemyState.Chase && HasLineOfSightToPlayer())
            {
                return;
            }

            float distToSound = Vector3.Distance(transform.position, soundPosition);

            // If the sound is within range, investigate it!
            if (distToSound <= noiseRadius)
            {
                targetInvestigatePosition = soundPosition;
                SetState(EnemyState.Investigate);
            }
        }

        #endregion

        #region Debug Gizmos

        private void OnDrawGizmosSelected()
        {
            Vector3 eyePos = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.6f;
            Vector3 forward = eyePoint != null ? eyePoint.forward : transform.forward;

            // FOV cone color
            Gizmos.color = currentState == EnemyState.Chase ? Color.red : (currentState == EnemyState.Investigate ? Color.yellow : Color.cyan);

            // Draw sight cone
            Vector3 leftRay = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * forward * viewDistance;
            Vector3 rightRay = Quaternion.Euler(0, viewAngle * 0.5f, 0) * forward * viewDistance;
            
            Gizmos.DrawRay(eyePos, leftRay);
            Gizmos.DrawRay(eyePos, rightRay);
            Gizmos.DrawRay(eyePos, forward * viewDistance);

            // If chasing or investigating, draw destination line
            if (currentState == EnemyState.Investigate)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetInvestigatePosition);
                Gizmos.DrawWireSphere(targetInvestigatePosition, 0.5f);
            }
            else if (currentState == EnemyState.Chase)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
                Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
            }
        }

        #endregion
    }
}
