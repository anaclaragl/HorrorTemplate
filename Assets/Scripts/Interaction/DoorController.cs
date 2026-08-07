using UnityEngine;

namespace HorrorTemplate.Interaction
{
    public class DoorController : MonoBehaviour
    {
        [Header("Door Movement")]
        [Tooltip("The angle (in degrees) to rotate the door when opened.")]
        [SerializeField] private float openAngle = 90.0f;
        [Tooltip("How fast the door swings open or closed.")]
        [SerializeField] private float smoothSpeed = 3.0f;
        [Tooltip("Is the door currently open?")]
        [SerializeField] private bool isOpen = false;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private Quaternion closedRotation;
        private Quaternion openRotation;

        public bool IsOpen => isOpen;

        private void Start()
        {
            closedRotation = transform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            // Smoothly swing the door toward target rotation
            Quaternion targetRot = isOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, smoothSpeed * Time.deltaTime);
        }

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            PlaySound(openSound);
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            PlaySound(closeSound);
        }

        public void Toggle()
        {
            isOpen = !isOpen;
            PlaySound(isOpen ? openSound : closeSound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
