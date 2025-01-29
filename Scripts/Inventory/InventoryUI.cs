using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public ItemDatabaseSO itemDatabase; // ✅ Reference Item Database to fetch items

    public void UpdateInventoryUI(Inventory inventory)
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var slot in inventory.localInventory)
        {
            GameObject newSlot = Instantiate(slotPrefab, inventoryPanel.transform);
            ItemSO item = itemDatabase.GetItemById(slot.itemId); // ✅ Fetch item from database

            if (item != null)
            {
                newSlot.GetComponentInChildren<Text>().text = $"{item.itemName} x{slot.quantity}";
                Image icon = newSlot.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = item.itemIcon;
            }
            else
            {
                Debug.LogError($"Item with ID {slot.itemId} not found in database!");
            }
        }
    }
}
