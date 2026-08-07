using UnityEngine;
using UnityEngine.Events;
using HorrorTemplate.Inventory;

namespace HorrorTemplate.Interaction
{
    public class LockedObject : Interactable
    {
        [Header("Key Settings")]
        [Tooltip("The ID of the item required to unlock this object (matches ItemData's itemId).")]
        [SerializeField] private string requiredItemId = "key_red";
        
        [Tooltip("Should the item be consumed (removed from inventory) when used?")]
        [SerializeField] private bool consumeItemOnUse = true;

        [Header("Messages")]
        [Tooltip("Message displayed when looking at the object while it is locked.")]
        [SerializeField] private string lockedPrompt = "Unlock Door";

        [Tooltip("Feedback message printed or shown when trying to interact without the key.")]
        [SerializeField] private string noKeyMessage = "It's locked. I need the Red Key.";

        [Tooltip("Message displayed when looking at the object once it has been unlocked.")]
        [SerializeField] private string unlockedPrompt = "Interact";

        [Header("State")]
        [SerializeField] private bool isLocked = true;

        [Header("Events")]
        [Tooltip("Fires when the object is successfully unlocked.")]
        [SerializeField] private UnityEvent OnUnlock;

        [Tooltip("Fires when the player attempts to interact but does not have the required key.")]
        [SerializeField] private UnityEvent OnLockedInteractionAttempt;

        [Tooltip("Fires if the player interacts with the object after it is already unlocked.")]
        [SerializeField] private UnityEvent OnUnlockedInteract;

        public bool IsLocked => isLocked;

        private void Start()
        {
            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            promptMessage = isLocked ? lockedPrompt : unlockedPrompt;
        }

        public override void Interact(GameObject player)
        {
            if (isLocked)
            {
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null && inventory.HasItem(requiredItemId))
                {
                    // Success: Unlock
                    isLocked = false;
                    UpdatePrompt();

                    Debug.Log($"[LockedObject] {gameObject.name} unlocked successfully using {requiredItemId}!");

                    if (consumeItemOnUse)
                    {
                        inventory.ConsumeCurrentItem();
                    }

                    OnUnlock?.Invoke();
                }
                else
                {
                    // Fail: No key
                    Debug.Log($"[LockedObject] Locked interaction failed: {noKeyMessage}");
                    OnLockedInteractionAttempt?.Invoke();
                }
            }
            else
            {
                // Already unlocked: Normal interaction
                Debug.Log($"[LockedObject] Interacted with unlocked {gameObject.name}");
                OnUnlockedInteract?.Invoke();
            }
        }

        public override string GetPrompt()
        {
            return isLocked ? lockedPrompt : unlockedPrompt;
        }

        /// <summary>
        /// Allows unlocking the object via external scripts/puzzles without needing the key.
        /// </summary>
        public void ForceUnlock()
        {
            if (!isLocked) return;
            isLocked = false;
            UpdatePrompt();
            OnUnlock?.Invoke();
        }
    }
}
