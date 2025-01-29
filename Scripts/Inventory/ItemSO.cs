using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;          // Unique ID for the item
    public string itemName;        // Name of the item
    public string itemDescription; // Description of the item
    public Sprite itemIcon;        // Icon for UI display
    public ItemType itemType;      // Type of the item
    public bool isStackable;       // Can the item stack?
    public int maxStackSize;       // Maximum stack size
    public GameObject itemPrefab;  // **Prefab reference for 3D equipment**

    public enum ItemType
    {
        Consumable,
        Equipment,
        Component,
        QuestItem,
        Misc
    }
}
