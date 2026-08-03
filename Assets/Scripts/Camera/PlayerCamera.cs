using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorTemplate.Core
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The parent player body transform to rotate horizontally.")]
        [SerializeField] private Transform playerBody;
        [Tooltip("The PlayerMovement component to check speeds and states.")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float lookSmoothing = 2.0f;
        [SerializeField] private float verticalClampMin = -85f;
        [SerializeField] private float verticalClampMax = 85f;

        [Header("Crouch Height Settings")]
        [SerializeField] private float standingCameraHeight = 1.8f;
        [SerializeField] private float crouchingCameraHeight = 0.8f;
        [SerializeField] private float crouchHeightLerpSpeed = 8.0f;

        [Header("Head Bob Settings")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequencyWalking = 14f;
        [SerializeField] private float bobAmplitudeWalking = 0.05f;
        [SerializeField] private float bobFrequencySprinting = 18f;
        [SerializeField] private float bobAmplitudeSprinting = 0.09f;
        [SerializeField] private float bobFrequencyCrouching = 10f;
        [SerializeField] private float bobAmplitudeCrouching = 0.02f;

        [Header("Breathing Sway Settings")]
        [SerializeField] private bool enableBreathingSway = true;
        [SerializeField] private float swayFrequency = 1.5f;
        [SerializeField] private float swayAmplitude = 0.015f;

        [Header("Input Setup")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string lookActionName = "Look";

        private InputAction lookAction;
        private float rotationX;
        private float bobTimer;
        private float currentCameraHeight;

        // Current smoothed look rotation values
        private Vector2 currentLookInput;
        private Vector2 smoothLookInput;

        private void Awake()
        {
            if (playerBody == null && transform.parent != null)
            {
                playerBody = transform.parent;
            }

            if (playerMovement == null && playerBody != null)
            {
                playerMovement = playerBody.GetComponent<PlayerMovement>();
            }

            if (playerInput == null && playerMovement != null)
            {
                playerInput = playerMovement.GetComponent<PlayerInput>();
            }

            currentCameraHeight = standingCameraHeight;
        }

        private void OnEnable()
        {
            InitializeInputs();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            lookAction?.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void InitializeInputs()
        {
            if (playerInput == null) return;

            var actionMap = playerInput.currentActionMap;
            if (actionMap == null) return;

            lookAction = actionMap.FindAction(lookActionName);
            lookAction?.Enable();
        }

        private void Update()
        {
            HandleCameraRotation();
            HandleCameraPosition();
        }

        private void HandleCameraRotation()
        {
            if (lookAction == null) return;

            Vector2 lookInput = lookAction.ReadValue<Vector2>();

            // Apply sensitivity
            currentLookInput.x = lookInput.x * mouseSensitivity;
            currentLookInput.y = lookInput.y * mouseSensitivity;

            // Apply smoothing
            smoothLookInput = Vector2.Lerp(smoothLookInput, currentLookInput, lookSmoothing * 10f * Time.deltaTime);

            // Calculate rotation
            rotationX -= smoothLookInput.y;
            rotationX = Mathf.Clamp(rotationX, verticalClampMin, verticalClampMax);

            // Vertical rotation (camera pitch)
            transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

            // Horizontal rotation (player body yaw)
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * smoothLookInput.x);
            }
        }

        private void HandleCameraPosition()
        {
            if (playerMovement == null) return;

            // Resolve Height based on crouching state
            float targetHeight = playerMovement.IsCrouching ? crouchingCameraHeight : standingCameraHeight;
            currentCameraHeight = Mathf.MoveTowards(currentCameraHeight, targetHeight, crouchHeightLerpSpeed * Time.deltaTime);

            Vector3 bobOffset = Vector3.zero;

            if (playerMovement.IsMoving && playerMovement.IsGrounded && enableHeadBob)
            {
                float frequency = bobFrequencyWalking;
                float amplitude = bobAmplitudeWalking;

                if (playerMovement.IsCrouching)
                {
                    frequency = bobFrequencyCrouching;
                    amplitude = bobAmplitudeCrouching;
                }
                else if (playerMovement.IsSprinting)
                {
                    frequency = bobFrequencySprinting;
                    amplitude = bobAmplitudeSprinting;
                }

                // Increment timer
                bobTimer += Time.deltaTime * frequency;

                // Side to side / up and down physics math
                float bobY = Mathf.Sin(bobTimer) * amplitude;
                float bobX = Mathf.Cos(bobTimer * 0.5f) * amplitude * 0.5f;

                bobOffset = new Vector3(bobX, bobY, 0f);
            }
            else
            {
                bobTimer = 0f;

                if (enableBreathingSway)
                {
                    float swayY = Mathf.Sin(Time.time * swayFrequency) * swayAmplitude;
                    float swayX = Mathf.Cos(Time.time * swayFrequency * 0.5f) * swayAmplitude * 0.3f;
                    bobOffset = new Vector3(swayX, swayY, 0f);
                }
            }

            transform.localPosition = new Vector3(0f, currentCameraHeight, 0f) + bobOffset;
        }
    }
}
