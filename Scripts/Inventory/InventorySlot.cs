using System;
using UnityEngine;
using Mirror;

[Serializable]
public struct InventorySlot
{
    public string itemId;
    public int quantity;

    public InventorySlot(string itemId, int quantity)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }
}
