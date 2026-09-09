using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections;

public class Inventory : NetworkBehaviour
{
    public class SyncListInventorySlot : SyncList<InventorySlot> { }

    public readonly SyncListInventorySlot slots = new SyncListInventorySlot();
    private string characterName;
    private bool inventoryLoaded = false;
    private static string savePath => Path.Combine(Application.persistentDataPath, "Characters");

    private void Start()
    {
        if (!isLocalPlayer) return;

        characterName = PlayerPrefs.GetString("SelectedCharacter", "DefaultPlayer");

        slots.OnAdd += OnItemAdded;
        slots.OnRemove += OnItemRemoved;
        slots.OnSet += OnItemChanged;
        slots.OnClear += OnInventoryCleared;

        LoadInventory();
    }

    private void OnItemAdded(int index)
    {
        Debug.Log($"🆕 Item added at index {index}");
        UpdateInventoryUI();
    }

    private void OnItemRemoved(int index, InventorySlot oldItem)
    {
        Debug.Log($"❌ Item removed at index {index}: {oldItem.itemId}");
        UpdateInventoryUI();
    }

    private void OnItemChanged(int index, InventorySlot oldItem)
    {
        Debug.Log($"🔄 Item changed at index {index}: {oldItem.itemId}");
        UpdateInventoryUI();
    }

    private void OnInventoryCleared()
    {
        Debug.Log($"🗑 Inventory cleared!");
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        InventoryUI ui = FindObjectOfType<InventoryUI>();
        if (ui != null) ui.UpdateInventoryUI();
    }


    public void LoadInventory()
    {
        if (inventoryLoaded) return;
        inventoryLoaded = true;

        if (!isLocalPlayer) return;

        if (string.IsNullOrEmpty(characterName))
        {
            characterName = PlayerPrefs.GetString("SelectedCharacter", "DefaultPlayer");
        }

        CharacterData characterData = CharacterData.Load(characterName);
        if (characterData == null)
        {
            Debug.LogError($"❌ Character data for {characterName} could not be loaded!");
            return;
        }

        if (characterData.Inventory == null)
        {
            Debug.LogWarning($"⚠ {characterName} has no inventory data! Starting with an empty inventory.");
            return; // No need to clear SyncList, just keep it empty
        }

        Debug.Log($"✅ Inventory Loaded for {characterName}. Items: {characterData.Inventory.Count}");

        // Call a server command to correctly load inventory
        CmdReloadInventory(characterData.Inventory);
    }


    [Command(requiresAuthority = false)]
    public void CmdAddItem(int itemId, int quantity, NetworkConnectionToClient sender = null)
    {
        InventorySlot slot = slots.Find(s => s.itemId == itemId);

        if (slot != null)
        {
            slot.quantity += quantity;
        }
        else
        {
            slots.Add(new InventorySlot(itemId, quantity));
        }

        Debug.Log($"✅ Item Added: {itemId} x{quantity}. Total Items: {slots.Count}");

        SaveCharacterInventory();

        // Pass the updated inventory list to client
        TargetReloadClientInventory(sender, slots.ToList());
    }


    [TargetRpc]
    private void TargetReloadClientInventory(NetworkConnection target, List<InventorySlot> newInventory)
    {
        if (!isLocalPlayer) return;

        Debug.Log($"🔄 Client-Side Inventory Reload Triggered");

        slots.Clear(); // Client modifies SyncList (Allowed)
        foreach (var slot in newInventory)
        {
            slots.Add(new InventorySlot(slot.itemId, slot.quantity)); // Client modifies SyncList (Allowed)
        }

        UpdateInventoryUI(); // Ensure UI is updated
    }


    [Command(requiresAuthority = false)]
    public void CmdRemoveItem(int itemId, int quantity, NetworkConnectionToClient sender = null)
    {
        InventorySlot slot = slots.Find(s => s.itemId == itemId);
        if (slot != null)
        {
            slot.quantity -= quantity;
            if (slot.quantity <= 0) slots.Remove(slot);
        }

        Debug.Log($"❌ Item Removed: {itemId} x{quantity}. Remaining Items: {slots.Count}");

        SaveCharacterInventory();

        // Pass the updated inventory list to client
        TargetReloadClientInventory(sender, slots.ToList());
    }

    private void ReloadCharacterData()
    {
        if (!isLocalPlayer) return; // Ensure only the local player runs this

        if (string.IsNullOrEmpty(characterName))
        {
            characterName = PlayerPrefs.GetString("SelectedCharacter", "DefaultPlayer");
        }

        CharacterData characterData = CharacterData.Load(characterName);
        if (characterData == null)
        {
            Debug.LogError($"❌ Failed to reload character data for {characterName}!");
            return;
        }

        Debug.Log($"🔄 Reloading character data for {characterName}...");

        // Call a [Command] to update SyncList safely
        CmdReloadInventory(characterData.Inventory);

        // Update UI
        UpdateInventoryUI();
    }

    [Command(requiresAuthority = false)]
    private void CmdReloadInventory(List<InventorySlot> newInventory, NetworkConnectionToClient sender = null)
    {
        // Pass inventory data correctly
        TargetReloadClientInventory(sender, newInventory); // Now passes `newInventory`
    }


    [ClientRpc]
    private void RpcUpdateInventoryUI()
    {
        UpdateInventoryUI();
    }

    public int GetItemCount(int itemId)
    {
        InventorySlot slot = slots.Find(s => s.itemId == itemId);
        return slot != null ? slot.quantity : 0;
    }

    public void SaveCharacterInventory()
    {
        if (string.IsNullOrEmpty(characterName))
        {
            characterName = PlayerPrefs.GetString("SelectedCharacter", "DefaultPlayer");
        }

        CharacterData characterData = CharacterData.Load(characterName);
        if (characterData == null)
        {
            Debug.LogError($"❌ Could not load character {characterName} before saving inventory!");
            return;
        }

        List<InventorySlot> savedSlots = new List<InventorySlot>();
        foreach (var slot in slots)
        {
            savedSlots.Add(new InventorySlot(slot.itemId, slot.quantity));
        }

        characterData.Inventory = savedSlots;
        characterData.Save();
        UpdateInventoryUI();
        Debug.Log($"✅ Inventory Saved for {characterName} with {savedSlots.Count} items!");
    }


    public void CmdPickupItem(NetworkIdentity pickupObject)
    {
        if (pickupObject == null)
        {
            Debug.LogError("❌ PickupObject is NULL!");
            return;
        }

        PickupItem pickupItem = pickupObject.GetComponent<PickupItem>();
        if (pickupItem == null)
        {
            Debug.LogError("❌ PickupObject has NO PickupItem component!");
            return;
        }

        Debug.Log($"📦 Picking up {pickupItem.quantity}x {pickupItem.itemId}");

        CmdAddItem(pickupItem.itemId, pickupItem.quantity);

        // Call the destroy function on PickupItem
        pickupItem.CmdDestroyPickup();
        }

    private HashSet<PickupItem> recentlyPickedUp = new HashSet<PickupItem>();

    private void OnTriggerEnter(Collider other)
    {
        if (!isOwned) return; // Ensure only the local player picks up

        PickupItem pickupItem = other.GetComponent<PickupItem>();
        if (pickupItem != null && !recentlyPickedUp.Contains(pickupItem))
        {
            Debug.Log($"🔹 {name} collided with a pickup item.");

            // Add to recently picked up items to prevent duplicates
            recentlyPickedUp.Add(pickupItem);

            // Call pickup command
            CmdPickupItem(pickupItem.netIdentity);

            // Remove from tracking after a short delay
            StartCoroutine(RemoveFromPickupCooldown(pickupItem));
        }
    }

    private IEnumerator RemoveFromPickupCooldown(PickupItem pickupItem)
    {
        yield return new WaitForSeconds(0.5f); // Adjust delay as needed
        recentlyPickedUp.Remove(pickupItem);
    }



    [Command(requiresAuthority = false)]
    public void CmdDropItem(int itemId, int quantity)
    {
        if (!slots.Any(s => s.itemId == itemId && s.quantity >= quantity))
        {
            Debug.LogError($"❌ {characterName} doesn't have {quantity}x {itemId}");
            return;
        }

        // Remove item from inventory
        CmdRemoveItem(itemId, quantity);

        // Spawn pickup prefab
        GameObject pickupPrefab = Instantiate(Resources.Load<GameObject>("PickupPrefab"));
        PickupItem pickupItem = pickupPrefab.GetComponent<PickupItem>();

        pickupItem.itemId = itemId; // Set the item's ID
        pickupItem.quantity = quantity; // Set quantity

        pickupPrefab.transform.position = transform.position + Vector3.forward * 1.5f;

        // Spawn on network
        NetworkServer.Spawn(pickupPrefab);

        Debug.Log($"✅ {characterName} dropped {quantity}x {itemId}");
    }
}

[System.Serializable]
public class InventorySlot
{
    public int itemId;
    public int quantity;

    public InventorySlot() { }

    public InventorySlot(int itemId, int quantity)
    {
        Debug.Log("Here be dragons");
        this.itemId = itemId;
        this.quantity = quantity;
    }
}
