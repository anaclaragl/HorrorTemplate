using UnityEngine;
using HorrorTemplate.Interaction;

namespace HorrorTemplate.Narrative
{
    public class ReadableDocument : Interactable
    {
        [Header("Document Settings")]
        [Tooltip("The ScriptableObject data containing the title and content of this readable document.")]
        [SerializeField] private DocumentData documentData;

        private void Start()
        {
            // Set dynamic prompt message if default is left untouched
            if (promptMessage == "Interact" && documentData != null)
            {
                promptMessage = "Read " + documentData.Title;
            }
        }

        public override void Interact(GameObject player)
        {
            if (documentData == null)
            {
                Debug.LogWarning($"ReadableDocument on {gameObject.name} is missing DocumentData.");
                return;
            }

            if (DocumentReaderUI.Instance != null)
            {
                DocumentReaderUI.Instance.ShowDocument(documentData);
            }
            else
            {
                Debug.LogError("ReadableDocument: DocumentReaderUI instance is missing in the scene. Please make sure the Document Reader UI is set up.");
            }
        }
    }
}
