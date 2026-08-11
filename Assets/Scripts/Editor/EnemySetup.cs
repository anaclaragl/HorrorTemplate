using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using HorrorTemplate.AI;

namespace HorrorTemplate.Editor
{
    public class EnemySetup : EditorWindow
    {
        [MenuItem("Horror Template/Create Enemy with Waypoints", false, 20)]
        public static void CreateEnemyInstance()
        {
            Vector3 spawnPosition = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                spawnPosition = SceneView.lastActiveSceneView.pivot;
                spawnPosition.y = 0;
            }

            // 1. Create Enemy capsule object
            GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObj.name = "Enemy";
            enemyObj.transform.position = spawnPosition + Vector3.up * 1.0f;

            // 2. Add and configure NavMeshAgent
            NavMeshAgent agent = enemyObj.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = enemyObj.AddComponent<NavMeshAgent>();
            }
            agent.speed = 2.0f;
            agent.angularSpeed = 180f;
            agent.acceleration = 8.0f;
            agent.stoppingDistance = 0.5f;
            agent.height = 2.0f;
            agent.radius = 0.5f;

            // 3. Create EyePoint Child
            GameObject eyePointObj = new GameObject("EyePoint");
            eyePointObj.transform.parent = enemyObj.transform;
            eyePointObj.transform.localPosition = new Vector3(0f, 0.6f, 0.35f);
            eyePointObj.transform.localRotation = Quaternion.identity;

            // 4. Add EnemyAI script
            EnemyAI enemyAI = enemyObj.AddComponent<EnemyAI>();

            // 5. Create Waypoints hierarchy in the scene
            GameObject waypointsRoot = new GameObject("Enemy_Waypoints");
            waypointsRoot.transform.position = spawnPosition;

            Transform[] waypoints = new Transform[3];
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(0f, 0f, 6f),
                new Vector3(6f, 0f, -4f),
                new Vector3(-6f, 0f, -4f)
            };

            for (int i = 0; i < 3; i++)
            {
                GameObject wp = new GameObject($"Waypoint_{i + 1}");
                wp.transform.parent = waypointsRoot.transform;
                wp.transform.position = spawnPosition + offsets[i];
                waypoints[i] = wp.transform;
            }

            // 6. Connect serialized properties automatically
            SerializedObject enemySO = new SerializedObject(enemyAI);
            
            SerializedProperty eyeProp = enemySO.FindProperty("eyePoint");
            if (eyeProp != null)
            {
                eyeProp.objectReferenceValue = eyePointObj.transform;
            }

            SerializedProperty patrolArrayProp = enemySO.FindProperty("patrolPoints");
            if (patrolArrayProp != null)
            {
                patrolArrayProp.arraySize = 3;
                for (int i = 0; i < 3; i++)
                {
                    patrolArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
                }
            }

            enemySO.ApplyModifiedProperties();

            // 7. Register Undo actions
            Undo.RegisterCreatedObjectUndo(enemyObj, "Create Horror Enemy");
            Undo.RegisterCreatedObjectUndo(waypointsRoot, "Create Enemy Waypoints");

            // Select created enemy in hierarchy
            Selection.activeGameObject = enemyObj;

            Debug.Log("EnemySetup: Enemy capsule and 3 connected Patrol Waypoints created successfully!");
        }
    }
}
