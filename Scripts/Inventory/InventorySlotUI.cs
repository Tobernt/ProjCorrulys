using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public Text itemQuantityText;

    public void SetupSlot(ItemSO item, int quantity)
    {
        if (item == null)
        {
            Debug.LogError("❌ Cannot set up slot: ItemSO is NULL!");
            return;
        }

        itemIcon.sprite = item.itemIcon;
        itemQuantityText.text = quantity > 1 ? quantity.ToString() : "";

        Debug.Log($"✅ Inventory Slot Updated: {item.itemId} x{quantity}");
    }
}
