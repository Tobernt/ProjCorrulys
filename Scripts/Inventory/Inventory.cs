using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory : NetworkBehaviour
{
    public int maxSlots = 20;
    public bool useServerStorage = false;
    public ItemDatabaseSO itemDatabase;

    public List<InventorySlot> localInventory = new List<InventorySlot>();

    // ✅ Use SyncList<InventorySlot> and register custom serialization
    public class SyncListInventorySlot : SyncList<InventorySlot> { }
    public readonly SyncListInventorySlot serverInventory = new SyncListInventorySlot();

    public override void OnStartServer() => Debug.Log("Inventory system started on server.");
    public override void OnStartClient() => Debug.Log("Inventory system started on client.");

    public void AddItem(string itemId, int quantity = 1)
    {
        ItemSO item = itemDatabase.GetItemById(itemId);
        if (item == null) return;

        if (useServerStorage)
            CmdAddItemToServer(itemId, quantity);
        else
            AddItemLocally(itemId, quantity);
    }

    private void AddItemLocally(string itemId, int quantity)
    {
        for (int i = 0; i < localInventory.Count; i++)
        {
            if (localInventory[i].itemId == itemId)
            {
                localInventory[i] = new InventorySlot(itemId, localInventory[i].quantity + quantity);
                return;
            }
        }

        if (localInventory.Count < maxSlots)
            localInventory.Add(new InventorySlot(itemId, quantity));
    }

    [Command]
    private void CmdAddItemToServer(string itemId, int quantity)
    {
        for (int i = 0; i < serverInventory.Count; i++)
        {
            if (serverInventory[i].itemId == itemId)
            {
                serverInventory[i] = new InventorySlot(itemId, serverInventory[i].quantity + quantity);
                RpcUpdateInventory();
                return;
            }
        }

        if (serverInventory.Count < maxSlots)
            serverInventory.Add(new InventorySlot(itemId, quantity));

        RpcUpdateInventory();
    }

    [ClientRpc]
    private void RpcUpdateInventory()
    {
        Debug.Log("Server inventory updated.");
    }
}
