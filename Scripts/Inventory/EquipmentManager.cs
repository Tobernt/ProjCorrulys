using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public Inventory playerInventory;
    public Dictionary<string, InventorySlot> equippedItems = new Dictionary<string, InventorySlot>();

    private void Start()
    {
        playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory == null)
        {
            Debug.LogError("❌ Player Inventory not found!");
        }
    }

    public void EquipItem(int itemId)
    {
        ItemSO itemData = FindObjectOfType<ItemDatabaseSO>().GetItemById(itemId.ToString());
        if (itemData == null) return;

        if (equippedItems.ContainsKey(itemData.itemType.ToString()))
        {
            Debug.Log($"🔄 Replacing {itemData.itemType}");
        }

        equippedItems[itemData.itemType.ToString()] = new InventorySlot(itemId, 1);
        Debug.Log($"✅ Equipped {itemData.itemName}");
    }

    public void UnequipItem(string itemType)
    {
        if (equippedItems.ContainsKey(itemType))
        {
            Debug.Log($"❌ Unequipped {itemType}");
            equippedItems.Remove(itemType);
        }
    }
}
