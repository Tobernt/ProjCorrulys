using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventorySaveSystem : MonoBehaviour
{
    private string saveFilePath => Path.Combine(Application.persistentDataPath, "Inventory.json");

    public void SaveInventory(Inventory inventory)
    {
        if (inventory.useServerStorage)
        {
            Debug.Log("Inventory is server-based, skipping local save.");
            return;
        }

        InventoryData data = new InventoryData();
        foreach (InventorySlot slot in inventory.localInventory)
        {
            data.slots.Add(new InventorySlotData
            {
                itemId = slot.itemId, // ✅ Use itemId instead of ItemSO
                quantity = slot.quantity
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Inventory saved locally to {saveFilePath}");
    }

    public void LoadInventory(Inventory inventory)
    {
        if (inventory.useServerStorage)
        {
            Debug.Log("Inventory is server-based, skipping local load.");
            return;
        }

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);

            inventory.localInventory.Clear();
            foreach (InventorySlotData slotData in data.slots)
            {
                inventory.localInventory.Add(new InventorySlot(slotData.itemId, slotData.quantity));
            }

            Debug.Log("Inventory loaded.");
        }
        else
        {
            Debug.LogWarning("No inventory save file found.");
        }
    }
}
