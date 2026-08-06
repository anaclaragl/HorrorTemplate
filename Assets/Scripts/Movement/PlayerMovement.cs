using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorTemplate.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Speed Settings")]
        [Tooltip("Normal walking speed in m/s.")]
        [SerializeField] private float walkSpeed = 2.5f;
        [Tooltip("Sprinting speed in m/s.")]
        [SerializeField] private float sprintSpeed = 4.5f;
        [Tooltip("Crouching speed in m/s.")]
        [SerializeField] private float crouchSpeed = 1.2f;

        [Header("Physics Settings")]
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float jumpHeight = 1.2f;
        [Tooltip("Can the player jump in this horror template?")]
        [SerializeField] private bool enableJumping = false;
        [Tooltip("Layer mask for ground detection and obstacles overhead.")]
        [SerializeField] private LayerMask obstacleMask = ~0;

        [Header("Crouch Settings")]
        [SerializeField] private float standingHeight = 2.0f;
        [SerializeField] private float crouchingHeight = 1.0f;
        [SerializeField] private float standingCenterY = 1.0f;
        [SerializeField] private float crouchingCenterY = 0.5f;
        [SerializeField] private float crouchTransitionSpeed = 8.0f;
        [Tooltip("Radius of the sphere cast to check for ceiling overhead when standing up.")]
        [SerializeField] private float ceilingCheckRadius = 0.35f;

        [Header("Footsteps Settings")]
        [SerializeField] private float walkFootstepInterval = 1.4f;
        [SerializeField] private float sprintFootstepInterval = 0.8f;
        [SerializeField] private float crouchFootstepInterval = 1.8f;

        [Header("Input Setup")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string sprintActionName = "Sprint";
        [SerializeField] private string crouchActionName = "Crouch";
        [SerializeField] private string jumpActionName = "Jump";

        // References
        private CharacterController controller;
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction jumpAction;

        // Movement state variables
        private Vector2 rawInputDirection;
        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeed;
        private float targetHeight;
        private float targetCenterY;
        private float footstepTimer;

        // State flags
        public bool IsCrouching { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsMoving { get; private set; }
        public Vector2 MoveInput => rawInputDirection;
        public float CurrentSpeed => currentSpeed;
        public bool IsGrounded => isGrounded;

        // Footstep Event
        // Parameters: Vector3 position, float loudness/intensity (0.3 for crouch, 1.0 for walk, 2.0 for sprint)
        public event Action<Vector3, float> OnFootstep;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            
            // Ensure default values are synced with the CharacterController config
            standingHeight = controller.height;
            standingCenterY = controller.center.y;
            targetHeight = standingHeight;
            targetCenterY = standingCenterY;

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
        }

        private void OnEnable()
        {
            InitializeInputs();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            sprintAction?.Disable();
            crouchAction?.Disable();
            jumpAction?.Disable();
        }

        private void InitializeInputs()
        {
            if (playerInput == null) return;

            if (playerInput.actions != null)
            {
                moveAction = playerInput.actions.FindAction(moveActionName);
                sprintAction = playerInput.actions.FindAction(sprintActionName);
                crouchAction = playerInput.actions.FindAction(crouchActionName);
                jumpAction = playerInput.actions.FindAction(jumpActionName);
            }

            if (moveAction == null && playerInput.currentActionMap != null)
            {
                var actionMap = playerInput.currentActionMap;
                moveAction = actionMap.FindAction(moveActionName);
                sprintAction = actionMap.FindAction(sprintActionName);
                crouchAction = actionMap.FindAction(crouchActionName);
                jumpAction = actionMap.FindAction(jumpActionName);
            }

            moveAction?.Enable();
            sprintAction?.Enable();
            crouchAction?.Enable();
            jumpAction?.Enable();
        }

        private void Update()
        {
            if (moveAction == null)
            {
                InitializeInputs();
            }

            HandleInputs();
            HandleGrounding();
            HandleCrouching();
            HandleMovementSpeed();
            MovePlayer();
            HandleFootsteps();
        }

        private void HandleInputs()
        {
            if (moveAction != null)
            {
                rawInputDirection = moveAction.ReadValue<Vector2>();
            }
            else
            {
                rawInputDirection = Vector2.zero;
            }

            IsMoving = rawInputDirection.magnitude > 0.01f;

            // Handle Crouch toggle
            if (crouchAction != null && crouchAction.triggered)
            {
                if (IsCrouching)
                {
                    if (CanStandUp())
                    {
                        IsCrouching = false;
                    }
                }
                else
                {
                    IsCrouching = true;
                }
            }

            // Handle Sprint
            if (sprintAction != null)
            {
                bool sprintHeld = sprintAction.IsPressed();
                if (sprintHeld && IsMoving && !IsCrouching)
                {
                    IsSprinting = true;
                }
                else
                {
                    IsSprinting = false;
                }
            }
            else
            {
                IsSprinting = false;
            }

            // Handle Jump
            if (enableJumping && jumpAction != null && jumpAction.triggered && !IsCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        private void HandleGrounding()
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        private void HandleCrouching()
        {
            targetHeight = IsCrouching ? crouchingHeight : standingHeight;
            targetCenterY = IsCrouching ? crouchingCenterY : standingCenterY;

            controller.height = Mathf.MoveTowards(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            
            Vector3 center = controller.center;
            center.y = Mathf.MoveTowards(center.y, targetCenterY, crouchTransitionSpeed * Time.deltaTime);
            controller.center = center;
        }

        public bool CanStandUp()
        {
            // Cast a sphere upwards from current position to verify no obstruction
            Vector3 start = transform.position + Vector3.up * crouchingHeight;
            float distance = standingHeight - crouchingHeight;

            if (Physics.SphereCast(start, ceilingCheckRadius, Vector3.up, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                return false; // Obstruction found
            }
            return true;
        }

        private void HandleMovementSpeed()
        {
            if (IsCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (IsSprinting)
            {
                currentSpeed = sprintSpeed;
            }
            else
            {
                currentSpeed = walkSpeed;
            }
        }

        private void MovePlayer()
        {
            Vector3 move = transform.right * rawInputDirection.x + transform.forward * rawInputDirection.y;
            move.y = 0;
            move.Normalize();

            controller.Move(move * currentSpeed * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleFootsteps()
        {
            if (!isGrounded || !IsMoving)
            {
                footstepTimer = 0f;
                return;
            }

            float stepInterval = walkFootstepInterval;
            float noiseLevel = 1.0f;

            if (IsCrouching)
            {
                stepInterval = crouchFootstepInterval;
                noiseLevel = 0.3f;
            }
            else if (IsSprinting)
            {
                stepInterval = sprintFootstepInterval;
                noiseLevel = 2.0f;
            }

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= stepInterval)
            {
                footstepTimer = 0f;
                TriggerFootstep(noiseLevel);
            }
        }

        private void TriggerFootstep(float noiseIntensity)
        {
            OnFootstep?.Invoke(transform.position, noiseIntensity);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 start = transform.position + Vector3.up * crouchingHeight;
            float distance = standingHeight - crouchingHeight;
            Gizmos.DrawWireSphere(start + Vector3.up * distance, ceilingCheckRadius);
        }
    }
}
