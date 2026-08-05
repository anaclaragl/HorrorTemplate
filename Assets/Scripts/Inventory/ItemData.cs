using UnityEngine;

namespace HorrorTemplate.Inventory
{
    [CreateAssetMenu(fileName = "New Item Data", menuName = "Horror Template/Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Properties")]
        [Tooltip("The unique identifier used to check for locks, key matches, etc.")]
        [SerializeField] private string itemId;
        
        [Tooltip("The readable name displayed on screen when looking at or holding the item.")]
        [SerializeField] private string displayName = "Key";

        [Header("Prefabs")]
        [Tooltip("The physical item prefab spawned in the world when dropped.")]
        [SerializeField] private GameObject physicalPrefab;

        [Tooltip("Optional. The model spawned in front of the camera when the player holds this item.")]
        [SerializeField] private GameObject handPrefab;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public GameObject PhysicalPrefab => physicalPrefab;
        public GameObject HandPrefab => handPrefab;
    }
}
