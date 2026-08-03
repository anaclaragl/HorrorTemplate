using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using HorrorTemplate.Core;

namespace HorrorTemplate.Editor
{
    public class PlayerSetup : EditorWindow
    {
        [MenuItem("Horror Template/Create Player", false, 10)]
        public static void CreatePlayerInstance()
        {
            // Check if there is already a Player in the scene
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                if (!EditorUtility.DisplayDialog("Player Already Exists",
                    "A GameObject with the 'Player' tag already exists in the scene. Creating another one might cause conflicts. Do you want to proceed?",
                    "Yes", "No"))
                {
                    return;
                }
            }

            // Create main Player GameObject
            GameObject playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            
            // Add CharacterController
            CharacterController charController = playerObj.AddComponent<CharacterController>();
            charController.height = 2.0f;
            charController.radius = 0.35f;
            charController.center = new Vector3(0f, 1.0f, 0f);
            charController.skinWidth = 0.08f;
            charController.minMoveDistance = 0f;

            // Add PlayerInput
            PlayerInput playerInput = playerObj.AddComponent<PlayerInput>();
            
            // Try to find the InputSystem_Actions.inputactions asset
            string[] guids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                if (inputActions != null)
                {
                    playerInput.actions = inputActions;
                    playerInput.defaultActionMap = "Player";
                }
            }
            else
            {
                Debug.LogWarning("PlayerSetup: InputSystem_Actions.inputactions asset not found in the project. Please assign your input actions manually on the PlayerInput component.");
            }

            // Add PlayerMovement
            PlayerMovement movement = playerObj.AddComponent<PlayerMovement>();

            // Create Camera Child GameObject
            GameObject cameraObj = new GameObject("Player Camera");
            cameraObj.transform.parent = playerObj.transform;
            cameraObj.transform.localPosition = new Vector3(0f, 1.8f, 0f); // Default eye level
            cameraObj.transform.localRotation = Quaternion.identity;
            cameraObj.tag = "MainCamera";

            // Add Camera
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.nearClipPlane = 0.01f; // Standard for horror games to see close objects clearly
            camera.farClipPlane = 1000f;

            // Add AudioListener (remove from scene main camera if it exists to avoid warnings)
            AudioListener sceneListener = Object.FindAnyObjectByType<AudioListener>();
            if (sceneListener != null && sceneListener.gameObject != cameraObj)
            {
                Debug.Log("PlayerSetup: Disabled pre-existing AudioListener on '" + sceneListener.gameObject.name + "' to avoid multiple AudioListener warnings.");
                sceneListener.enabled = false;
            }
            cameraObj.AddComponent<AudioListener>();

            // Add PlayerCamera
            PlayerCamera playerCamera = cameraObj.AddComponent<PlayerCamera>();

            // Set up inspector references explicitly
            SerializedObject movementSO = new SerializedObject(movement);
            SerializedProperty inputProp = movementSO.FindProperty("playerInput");
            if (inputProp != null)
            {
                inputProp.objectReferenceValue = playerInput;
                movementSO.ApplyModifiedProperties();
            }

            SerializedObject cameraSO = new SerializedObject(playerCamera);
            SerializedProperty bodyProp = cameraSO.FindProperty("playerBody");
            SerializedProperty movementRefProp = cameraSO.FindProperty("playerMovement");
            SerializedProperty cameraInputProp = cameraSO.FindProperty("playerInput");
            if (bodyProp != null) bodyProp.objectReferenceValue = playerObj.transform;
            if (movementRefProp != null) movementRefProp.objectReferenceValue = movement;
            if (cameraInputProp != null) cameraInputProp.objectReferenceValue = playerInput;
            cameraSO.ApplyModifiedProperties();

            // Register created object with Undo system
            Undo.RegisterCreatedObjectUndo(playerObj, "Create Horror Player");
            
            // Focus on the new Player object
            Selection.activeGameObject = playerObj;

            Debug.Log("PlayerSetup: Player GameObject successfully created and configured!");
        }
    }
}
