using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorTemplate.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The camera to cast the ray from. If null, will search for the main camera.")]
        [SerializeField] private Camera playerCamera;

        [Header("Raycast Settings")]
        [Tooltip("Default max distance for raycast (interactable-specific distances will clamp to this or override it).")]
        [SerializeField] private float defaultMaxDistance = 2.0f;
        [SerializeField] private LayerMask interactableLayer = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Input Setup")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string interactActionName = "Interact";

        [SerializeField] private InputAction interactAction;
        [SerializeField] private Interactable currentTarget;

        // Event triggered when looked-at target changes. Sends the new target (or null if looking at nothing).
        public event Action<Interactable> OnInteractableTargetChanged;

        public Interactable CurrentTarget => currentTarget;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

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
            interactAction?.Disable();
            ClearCurrentTarget();
        }

        private void InitializeInputs()
        {
            if (playerInput == null) { Debug.Log("No player input"); return; }

            if (playerInput.actions != null)
            {
                interactAction = playerInput.actions.FindAction(interactActionName);
                Debug.Log("player input");
            }

            if (interactAction == null && playerInput.currentActionMap != null)
            {
                Debug.Log("player input 2");
                interactAction = playerInput.currentActionMap.FindAction(interactActionName);
            }

            interactAction?.Enable();
        }

        private void Update()
        {
            if (interactAction == null)
            {
                InitializeInputs();
            }

            PerformRaycast();
            HandleInteractionInput();
        }

        private void PerformRaycast()
        {
            if (playerCamera == null) return;

            // Ray from center of camera viewport
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            // Get all hits along the ray
            RaycastHit[] hits = Physics.RaycastAll(ray, defaultMaxDistance, interactableLayer, triggerInteraction);
            
            // Sort hits by distance
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (RaycastHit hit in hits)
            {
                // Ignore hits on the player itself (or its children)
                if (hit.collider.transform.root == transform.root)
                {
                    continue;
                }

                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable == null)
                {
                    interactable = hit.collider.GetComponent<Interactable>();
                }

                if (interactable != null)
                {
                    float distanceToCollider = hit.distance;
                    if (distanceToCollider <= interactable.GetInteractionDistance())
                    {
                        if (interactable != currentTarget)
                        {
                            SetNewTarget(interactable);
                        }
                        return;
                    }
                }

                // If the first non-player object we hit is not interactable,
                // it means a wall or obstacle is blocking our view.
                break;
            }

            if (currentTarget != null)
            {
                ClearCurrentTarget();
            }
        }

        private void SetNewTarget(Interactable target)
        {
            if (currentTarget != null)
            {
                currentTarget.OnHoverExit(gameObject);
            }

            currentTarget = target;
            currentTarget.OnHoverEnter(gameObject);
            OnInteractableTargetChanged?.Invoke(currentTarget);
        }

        private void ClearCurrentTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.OnHoverExit(gameObject);
            }
            currentTarget = null;
            OnInteractableTargetChanged?.Invoke(null);
        }

        private void HandleInteractionInput()
        {
            if (interactAction != null && interactAction.WasPressedThisFrame())
            {
                Debug.Log($"[PlayerInteraction] Pressed Interact key. Target under crosshair: {(currentTarget != null ? currentTarget.gameObject.name : "None")}");
                if (currentTarget != null)
                {
                    currentTarget.Interact(gameObject);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Camera cam = playerCamera;
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam == null) return;

            Gizmos.color = (currentTarget != null) ? Color.green : Color.red;

            Vector3 start = cam.transform.position;
            Vector3 end = start + cam.transform.forward * defaultMaxDistance;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(end, 0.05f);

            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(currentTarget.transform.position, Vector3.one * 1f);
            }
        }
    }
}
