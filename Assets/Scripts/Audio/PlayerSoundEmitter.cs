using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorTemplate.Audio
{
    public class PlayerSoundEmitter : MonoBehaviour
    {
        /// <summary>
        /// Event fired whenever a sound or noise is emitted in the world.
        /// Parameters: Vector3 soundPosition, float noiseRadius (in meters).
        /// </summary>
        public static event Action<Vector3, float> OnSoundEmitted;

        private struct SoundDebugData
        {
            public Vector3 position;
            public float radius;
            public float expireTime;
        }

        private static readonly List<SoundDebugData> activeSoundGizmos = new List<SoundDebugData>();
        private static PlayerSoundEmitter instance;

        [Header("Debug Settings")]
        [Tooltip("How long the sound radius wireframe sphere stays visible in the Scene view.")]
        [SerializeField] private float gizmoDuration = 1.5f;
        
        [Tooltip("Color of the wireframe sphere drawn in the Scene view.")]
        [SerializeField] private Color gizmoColor = new Color(1f, 0.6f, 0f, 0.85f); // Orange

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Emits a noise in the game world that can alert nearby enemies.
        /// </summary>
        /// <param name="position">The origin point of the noise.</param>
        /// <param name="noiseRadius">How far the noise travels in meters.</param>
        public static void EmitSound(Vector3 position, float noiseRadius)
        {
            if (noiseRadius <= 0.01f) return;

            OnSoundEmitted?.Invoke(position, noiseRadius);

            float duration = instance != null ? instance.gizmoDuration : 1.5f;
            activeSoundGizmos.Add(new SoundDebugData
            {
                position = position,
                radius = noiseRadius,
                expireTime = Time.time + duration
            });
        }

        private void OnDrawGizmos()
        {
            if (activeSoundGizmos.Count == 0) return;

            Gizmos.color = gizmoColor;

            for (int i = activeSoundGizmos.Count - 1; i >= 0; i--)
            {
                SoundDebugData data = activeSoundGizmos[i];
                if (Time.time <= data.expireTime)
                {
                    Gizmos.DrawWireSphere(data.position, data.radius);
                }
                else
                {
                    activeSoundGizmos.RemoveAt(i);
                }
            }
        }
    }
}
