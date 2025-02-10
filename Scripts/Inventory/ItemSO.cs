using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public ItemType itemType;
    public bool isStackable;
    public int maxStackSize;
    public GameObject itemPrefab;

    public int damage;
    public float attackSpeed;
    public int defense;
    public RarityType rarity;

    public enum ItemType
    {
        Consumable,
        Equipment,
        Component,
        QuestItem,
        Misc,
        Helmet,
        Gloves,
        Chest,
        Legs,
        Boots,
        Weapon,
        Shield,
        Necklace,
        Ring
    }


    public enum RarityType
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
