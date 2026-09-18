using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorTemplate.Gameplay
{
    [RequireComponent(typeof(Light))]
    public class Flashlight : MonoBehaviour
    {
        [Header("Light Settings")]
        [Tooltip("The maximum intensity of the flashlight when fully charged.")]
        [SerializeField] private float maxIntensity = 10.0f;
        [Tooltip("The minimum intensity before the flashlight turns off completely.")]
        [SerializeField] private float minIntensity = 0.5f;

        [Header("Battery Settings")]
        [SerializeField] private float maxBattery = 100.0f;
        [Tooltip("How much battery is drained per second when the flashlight is ON.")]
        [SerializeField] private float drainRate = 1.5f;
        
        [Header("Input Setup")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string toggleActionName = "ToggleFlashlight";

        private Light spotLight;
        private InputAction toggleAction;
        
        public bool IsOn { get; private set; }
        public float CurrentBattery { get; private set; }

        private void Awake()
        {
            spotLight = GetComponent<Light>();
            spotLight.type = LightType.Spot;
            CurrentBattery = maxBattery;
            IsOn = spotLight.enabled;
        }

        private void OnEnable()
        {
            InitializeInputs();
        }

        private void OnDisable()
        {
            toggleAction?.Disable();
        }

        private void InitializeInputs()
        {
            if (playerInput == null)
            {
                playerInput = GetComponentInParent<PlayerInput>();
            }

            if (playerInput != null && playerInput.actions != null)
            {
                toggleAction = playerInput.actions.FindAction(toggleActionName);
                if (toggleAction == null && playerInput.currentActionMap != null)
                {
                    toggleAction = playerInput.currentActionMap.FindAction(toggleActionName);
                }
            }

            toggleAction?.Enable();
        }

        private void Update()
        {
            if (toggleAction != null && toggleAction.triggered)
            {
                Toggle();
            }

            if (IsOn)
            {
                DrainBattery();
                UpdateLightIntensity();
            }
        }

        public void Toggle()
        {
            if (CurrentBattery <= 0)
            {
                IsOn = false;
                spotLight.enabled = false;
                return;
            }

            IsOn = !IsOn;
            spotLight.enabled = IsOn;
            
            // Note: Add toggle sound effect call here if needed
        }

        private void DrainBattery()
        {
            if (CurrentBattery > 0)
            {
                CurrentBattery -= drainRate * Time.deltaTime;
                
                if (CurrentBattery <= 0)
                {
                    CurrentBattery = 0;
                    Toggle(); // Turn off automatically
                }
            }
        }

        private void UpdateLightIntensity()
        {
            // Dim the light linearly based on battery percentage
            float batteryPct = CurrentBattery / maxBattery;
            spotLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, batteryPct);
        }

        public void Recharge(float amount)
        {
            CurrentBattery += amount;
            if (CurrentBattery > maxBattery)
            {
                CurrentBattery = maxBattery;
            }
            
            // If it was off and we gained battery, keep it off but update intensity for when it turns on
            if (!IsOn)
            {
                UpdateLightIntensity();
            }
        }
    }
}
