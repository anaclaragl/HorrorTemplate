using UnityEngine;

namespace HorrorTemplate.Interaction
{
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("The text displayed on the UI when looking at this object.")]
        [SerializeField] protected string promptMessage = "Interact";
        
        [Tooltip("The maximum distance from which the player can interact with this specific object.")]
        [SerializeField] protected float interactionDistance = 2.0f;

        /// <summary>
        /// Called when the player presses the interact key while looking at this object.
        /// </summary>
        /// <param name="player">The player GameObject initiating the interaction.</param>
        public abstract void Interact(GameObject player);

        /// <summary>
        /// Gets the prompt message displayed for this interactable.
        /// </summary>
        public virtual string GetPrompt()
        {
            return promptMessage;
        }

        /// <summary>
        /// Gets the maximum distance allowed for interaction.
        /// </summary>
        public virtual float GetInteractionDistance()
        {
            return interactionDistance;
        }

        /// <summary>
        /// Called when the player starts looking directly at this interactable.
        /// Useful for triggering outline shaders, highlighting, or UI hover events.
        /// </summary>
        public virtual void OnHoverEnter(GameObject player)
        {
        }

        /// <summary>
        /// Called when the player stops looking at this interactable.
        /// </summary>
        public virtual void OnHoverExit(GameObject player)
        {
        }
    }
}
