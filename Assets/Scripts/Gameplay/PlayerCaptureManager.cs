using System.Collections;
using UnityEngine;

namespace HorrorTemplate.Gameplay
{
    public class PlayerCaptureManager : MonoBehaviour
    {
        [Header("Capture Settings")]
        [Tooltip("How many times the player can be captured before it's a Game Over.")]
        [SerializeField] private int maxCaptures = 3;
        
        [Tooltip("The spawn point to relocate the player after a capture.")]
        [SerializeField] private Transform safeRespawnPoint;

        [Header("Effects")]
        [Tooltip("How long the player is stunned/disoriented after respawning.")]
        [SerializeField] private float disorientationDuration = 3.0f;

        private int currentCaptures = 0;
        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public void CapturePlayer()
        {
            currentCaptures++;
            Debug.Log($"Player captured! ({currentCaptures}/{maxCaptures})");

            if (currentCaptures >= maxCaptures)
            {
                HandleGameOver();
            }
            else
            {
                StartCoroutine(SoftCaptureRoutine());
            }
        }

        private IEnumerator SoftCaptureRoutine()
        {
            // Disable player movement temporarily
            if (characterController != null)
                characterController.enabled = false;

            // Optional: Add screen fade out here if a UI manager exists
            
            // Relocate player
            if (safeRespawnPoint != null)
            {
                transform.position = safeRespawnPoint.position;
                transform.rotation = safeRespawnPoint.rotation;
            }
            else
            {
                Debug.LogWarning("No Safe Respawn Point assigned in PlayerCaptureManager!");
            }

            // Wait a frame to ensure physics update
            yield return null;

            // Re-enable movement
            if (characterController != null)
                characterController.enabled = true;

            // Simulate disorientation (could link to post-processing later)
            Debug.Log("Player is disoriented...");
            yield return new WaitForSeconds(disorientationDuration);
            Debug.Log("Player recovered.");
        }

        private void HandleGameOver()
        {
            Debug.LogError("GAME OVER - Max captures reached!");
            // In the future: Load Game Over scene or show Game Over UI
        }
    }
}
