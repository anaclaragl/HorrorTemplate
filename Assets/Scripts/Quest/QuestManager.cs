using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorTemplate.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        private HashSet<string> completedObjectives = new HashSet<string>();

        public event Action<string> OnObjectiveCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Optional: DontDestroyOnLoad(gameObject); if quests span multiple scenes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CompleteObjective(string objectiveId)
        {
            if (string.IsNullOrEmpty(objectiveId)) return;

            if (!completedObjectives.Contains(objectiveId))
            {
                completedObjectives.Add(objectiveId);
                Debug.Log($"[QuestManager] Objective Completed: {objectiveId}");
                OnObjectiveCompleted?.Invoke(objectiveId);
            }
        }

        public bool IsObjectiveComplete(string objectiveId)
        {
            return completedObjectives.Contains(objectiveId);
        }

        public void ResetObjectives()
        {
            completedObjectives.Clear();
        }
    }
}
