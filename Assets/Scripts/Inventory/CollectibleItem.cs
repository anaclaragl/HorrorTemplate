using UnityEngine;
using HorrorTemplate.Interaction;

namespace HorrorTemplate.Inventory
{
    public class CollectibleItem : Interactable
    {
        [Header("Item Config")]
        [Tooltip("The ScriptableObject data defining this item.")]
        [SerializeField] private ItemData itemData;

        public ItemData ItemData => itemData;

        private void Start()
        {
            // Dynamically build a convenient prompt if left as default
            if (promptMessage == "Interact" && itemData != null)
            {
                promptMessage = "Take " + itemData.DisplayName;
            }
        }

        public override void Interact(GameObject player)
        {
            if (itemData == null)
            {
                Debug.LogWarning($"CollectibleItem on {gameObject.name}: ItemData is not assigned.");
                return;
            }

            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.EquipItem(itemData);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"CollectibleItem: Player {player.name} is missing PlayerInventory component.");
            }
        }
    }
}
