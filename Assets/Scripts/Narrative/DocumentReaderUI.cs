using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using HorrorTemplate.Core;

namespace HorrorTemplate.Narrative
{
    public class DocumentReaderUI : MonoBehaviour
    {
        public static DocumentReaderUI Instance { get; private set; }

        [Header("UI Components")]
        [Tooltip("The main container panel of the document reader UI.")]
        [SerializeField] private GameObject uiPanel;
        
        [Tooltip("TextMeshProUGUI component for displaying the document's title.")]
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Tooltip("TextMeshProUGUI component for displaying the document's text body.")]
        [SerializeField] private TextMeshProUGUI contentText;

        [Tooltip("Optional. TextMeshProUGUI component displaying instructions on how to close the document.")]
        [SerializeField] private TextMeshProUGUI closePromptText;

        private bool isOpen;
        private float openTime;
        private PlayerMovement cachedMovement;
        private PlayerCamera cachedCamera;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isOpen) return;

            // Prevent closing instantly in the same frame as opening
            if (Time.time - openTime < 0.2f) return;

            // Close on left click, Escape key, or E key
            if ((Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                HideDocument();
            }
        }

        /// <summary>
        /// Displays the reading panel filled with the document data and pauses player control.
        /// </summary>
        public void ShowDocument(DocumentData doc)
        {
            if (doc == null) return;

            isOpen = true;
            openTime = Time.time;

            if (titleText != null) titleText.text = doc.Title;
            if (contentText != null) contentText.text = doc.Content;
            
            if (closePromptText != null)
            {
                closePromptText.text = "[Click or Press E to Close]";
            }

            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }

            // Disable player movement and camera looking
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                cachedMovement = player.GetComponent<PlayerMovement>();
                if (cachedMovement != null) cachedMovement.enabled = false;

                cachedCamera = player.GetComponentInChildren<PlayerCamera>();
                if (cachedCamera != null) cachedCamera.enabled = false;
            }

            // Show cursor so player can click to close
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Closes the reading panel and restores player control.
        /// </summary>
        public void HideDocument()
        {
            isOpen = false;
            
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }

            // Restore player control
            if (cachedMovement != null)
            {
                cachedMovement.enabled = true;
                cachedMovement = null;
            }

            if (cachedCamera != null)
            {
                cachedCamera.enabled = true;
                cachedCamera = null;
            }

            // Relock cursor for first-person gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
