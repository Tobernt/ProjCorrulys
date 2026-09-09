using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject inventorySlotPrefab;
    public Transform inventoryContainer;
    public ItemDatabaseSO itemDatabase; //Assigned in Inspector
    public int maxSlots = 20;
    private Inventory playerInventory;


    private void Start()
    {
        // Start retrying until we find the player's inventory
        InvokeRepeating(nameof(FindPlayerInventory), 0.5f, 1.0f);

        if (inventoryPanel == null)
        {
            Debug.LogError("❌ Inventory Panel is NULL! Assign it in the inspector.");
        }

        if (itemDatabase == null)
        {
            Debug.LogError("❌ ItemDatabaseSO is NOT assigned in the Inspector!");
        }
    }

    private void FindPlayerInventory()
    {
        playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory != null)
        {
            Debug.Log("✅ Inventory Component Found!");
            CancelInvoke(nameof(FindPlayerInventory)); // Stop retrying once found
        }
        else
        {
            Debug.LogWarning("❌ Inventory component not found! Retrying...");
        }
    }

    public bool IsInventoryOpen()
    {
        return inventoryPanel != null && inventoryPanel.activeSelf;
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("❌ Inventory Panel is NULL! Assign it in the inspector.");
            return;
        }

        bool isOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            UpdateInventoryUI();
        }

        Debug.Log($"📂 Inventory UI Toggled: {(isOpen ? "Open" : "Closed")}");
    }

    public void UpdateInventoryUI()
    {
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<Inventory>(); // Try to find inventory again
            if (playerInventory == null)
            {
                Debug.LogWarning("⚠ Inventory not found! Retrying...");
                Invoke(nameof(UpdateInventoryUI), 0.5f);
                return;
            }
        }

        if (playerInventory.slots == null)
        {
            Debug.LogError("❌ Inventory slots are NULL! UI cannot update.");
            return;
        }

        Debug.Log($"🔍 Slot 1 Quantity: {(playerInventory.slots.Count > 0 ? playerInventory.slots[0].quantity : 0)}");

        if (itemDatabase == null)
        {
            Debug.LogError("❌ ItemDatabaseSO is NULL! Assign it in the Inspector.");
            return;
        }

        // Ensure we have the correct number of slots
        while (inventoryContainer.childCount < maxSlots)
        {
            Instantiate(inventorySlotPrefab, inventoryContainer);
        }

        int index = 0;
        foreach (Transform child in inventoryContainer)
        {
            InventorySlotUI slotUI = child.GetComponent<InventorySlotUI>();
            if (slotUI == null) continue;

            if (index < playerInventory.slots.Count)
            {
                InventorySlot slot = playerInventory.slots[index];
                ItemSO itemData = itemDatabase.GetItemById(slot.itemId.ToString());

                if (itemData != null)
                {
                    slotUI.SetupSlot(itemData, slot.quantity);
                    slotUI.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError($"❌ Item with ID {slot.itemId} not found in ItemDatabaseSO!");
                }
            }
            else
            {
                // Keep empty slots visible, but clear any existing content
                slotUI.itemIcon.sprite = null;
                slotUI.itemQuantityText.text = "";
                slotUI.gameObject.SetActive(true);
            }

            index++;
        }

        Debug.Log("🔄 Inventory UI Updated!");
    }
}
