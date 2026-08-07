using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorTemplate.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The parent transform in front of the camera where the held item's model is attached.")]
        [SerializeField] private Transform handPivot;
        [Tooltip("The main camera used to determine dropping forward direction.")]
        [SerializeField] private Transform playerCamera;

        [Header("Drop Settings")]
        [Tooltip("Key to press to drop the currently held item.")]
        [SerializeField] private Key dropKey = Key.G;
        [Tooltip("How much force to apply forward when tossing a dropped item.")]
        [SerializeField] private float dropTossForce = 2.0f;
        [Tooltip("Offset distance in front of player where item is spawned when dropped.")]
        [SerializeField] private float dropForwardOffset = 0.8f;
        [Tooltip("Vertical height offset relative to player transform where item is spawned when dropped.")]
        [SerializeField] private float dropHeightOffset = 1.0f;

        // Active state
        private ItemData currentItem;
        private GameObject spawnedHandModel;

        // Events
        public event Action<ItemData> OnItemEquipped;
        public event Action<ItemData> OnItemDropped;

        public ItemData CurrentItem => currentItem;
        public bool IsHoldingItem => currentItem != null;

        private void Awake()
        {
            if (playerCamera == null)
            {
                var mainCam = Camera.main;
                if (mainCam != null) playerCamera = mainCam.transform;
            }
        }

        private void Update()
        {
            HandleDropInput();
        }

        private void HandleDropInput()
        {
            if (Keyboard.current != null && Keyboard.current[dropKey].wasPressedThisFrame)
            {
                if (IsHoldingItem)
                {
                    DropCurrentItem();
                }
            }
        }

        /// <summary>
        /// Equips a new item. If already holding an item, it automatically drops it.
        /// </summary>
        public void EquipItem(ItemData item)
        {
            if (item == null) return;

            if (IsHoldingItem)
            {
                DropCurrentItem();
            }

            currentItem = item;

            if (handPivot != null)
            {
                GameObject prefabToSpawn = currentItem.HandPrefab != null ? currentItem.HandPrefab : currentItem.PhysicalPrefab;
                if (prefabToSpawn != null)
                {
                    spawnedHandModel = Instantiate(prefabToSpawn, handPivot);
                    spawnedHandModel.transform.localPosition = Vector3.zero;
                    spawnedHandModel.transform.localRotation = Quaternion.identity;
                    
                    DisableCollidersAndPhysics(spawnedHandModel);
                }
            }

            OnItemEquipped?.Invoke(currentItem);
            Debug.Log($"Inventory: Equipped item {currentItem.DisplayName}");
        }

        /// <summary>
        /// Drops the current item back into the world.
        /// </summary>
        public void DropCurrentItem()
        {
            if (!IsHoldingItem) return;

            ItemData itemToDrop = currentItem;

            if (spawnedHandModel != null)
            {
                Destroy(spawnedHandModel);
                spawnedHandModel = null;
            }

            if (itemToDrop.PhysicalPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * dropHeightOffset;
                Vector3 dropDirection = transform.forward;

                if (playerCamera != null)
                {
                    spawnPos = playerCamera.position + playerCamera.forward * dropForwardOffset;
                    dropDirection = playerCamera.forward;
                }
                else
                {
                    spawnPos += transform.forward * dropForwardOffset;
                }

                GameObject physicalObj = Instantiate(itemToDrop.PhysicalPrefab, spawnPos, Quaternion.identity);

                Rigidbody rb = physicalObj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = physicalObj.GetComponentInChildren<Rigidbody>();
                }

                if (rb != null)
                {
                    Vector3 forceVec = (dropDirection + Vector3.up * 0.2f).normalized * dropTossForce;
                    rb.AddForce(forceVec, ForceMode.Impulse);
                }
            }
            else
            {
                Debug.LogWarning($"Inventory: Physical prefab is not assigned for item {itemToDrop.DisplayName}");
            }

            currentItem = null;
            OnItemDropped?.Invoke(itemToDrop);
            Debug.Log($"Inventory: Dropped item {itemToDrop.DisplayName}");
        }

        /// <summary>
        /// Consumes the currently held item (destroys it without dropping it into the world).
        /// </summary>
        public void ConsumeCurrentItem()
        {
            if (!IsHoldingItem) return;

            ItemData itemToConsume = currentItem;

            if (spawnedHandModel != null)
            {
                Destroy(spawnedHandModel);
                spawnedHandModel = null;
            }

            currentItem = null;
            OnItemDropped?.Invoke(itemToConsume);
            Debug.Log($"Inventory: Consumed item {itemToConsume.DisplayName}");
        }

        /// <summary>
        /// Returns true if player is holding the item with specified ID.
        /// </summary>
        public bool HasItem(string itemId)
        {
            return currentItem != null && currentItem.ItemId == itemId;
        }

        private void DisableCollidersAndPhysics(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            Rigidbody[] rigidbodies = obj.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }
        }
    }
}
