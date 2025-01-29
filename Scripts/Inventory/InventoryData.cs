using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class InventoryData
{
    public List<InventorySlotData> slots = new List<InventorySlotData>();
}

[System.Serializable]
public class InventorySlotData
{
    public string itemId;
    public int quantity;
}
