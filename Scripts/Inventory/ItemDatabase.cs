using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    public ItemSO GetItemById(string itemId)
    {
        foreach (var item in items)
        {
            if (item.itemId == itemId)
                return item;
        }

        Debug.LogError($"Item with ID {itemId} not found in the database.");
        return null;
    }
}
