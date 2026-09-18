using UnityEngine;
using UnityEngine.Events;

namespace HorrorTemplate.Quest
{
    public class ObjectiveTrigger : MonoBehaviour
    {
        [Header("Objective Settings")]
        [Tooltip("The unique ID of the objective to complete.")]
        [SerializeField] private string objectiveId;
        
        [Tooltip("If true, this trigger will only fire once.")]
        [SerializeField] private bool triggerOnce = true;
        
        [Tooltip("Only GameObjects with this tag can trigger the objective (e.g., Player).")]
        [SerializeField] private string targetTag = "Player";

        [Header("Events")]
        public UnityEvent OnObjectiveTriggered;

        private bool hasTriggered = false;

        public void TriggerObjectiveManual()
        {
            if (triggerOnce && hasTriggered) return;

            hasTriggered = true;
            
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteObjective(objectiveId);
            }
            
            OnObjectiveTriggered?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
                return;

            TriggerObjectiveManual();
        }
    }
}
