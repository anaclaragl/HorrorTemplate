using UnityEngine;

namespace HorrorTemplate.Narrative
{
    [CreateAssetMenu(fileName = "New Document Data", menuName = "Horror Template/Narrative/Document Data")]
    public class DocumentData : ScriptableObject
    {
        [Header("Document Metadata")]
        [Tooltip("Unique ID for this document, useful for saving state or checking if read.")]
        [SerializeField] private string documentId;
        
        [Tooltip("Title of the document (displayed at the top of the reading panel).")]
        [SerializeField] private string title = "Letter";

        [Header("Content")]
        [Tooltip("The actual text content of the document.")]
        [TextArea(10, 20)]
        [SerializeField] private string content = "Dear friend,\n\nYou should not have come here...";

        public string DocumentId => documentId;
        public string Title => title;
        public string Content => content;
    }
}
