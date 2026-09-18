using UnityEngine;

namespace HorrorTemplate.Interaction
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioInteractable : Interactable
    {
        [Header("Audio Settings")]
        [Tooltip("The audio source that will be toggled.")]
        [SerializeField] private AudioSource targetAudio;
        
        [Tooltip("Prompt when the audio is currently OFF.")]
        [SerializeField] private string turnOnPrompt = "Turn On";
        
        [Tooltip("Prompt when the audio is currently ON.")]
        [SerializeField] private string turnOffPrompt = "Turn Off";

        private bool isPlaying = false;

        private void Awake()
        {
            if (targetAudio == null)
            {
                targetAudio = GetComponent<AudioSource>();
            }

            isPlaying = targetAudio.isPlaying;
        }

        public override void Interact(GameObject player)
        {
            if (isPlaying)
            {
                targetAudio.Stop();
                isPlaying = false;
            }
            else
            {
                targetAudio.Play();
                isPlaying = true;
            }
        }

        public override string GetPrompt()
        {
            return isPlaying ? turnOffPrompt : turnOnPrompt;
        }
    }
}
