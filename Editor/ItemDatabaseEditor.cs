using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(ItemDatabaseSO))]
public class ItemDatabaseEditor : Editor
{
    private enum Tab
    {
        Create,
        Edit,
        ItemList
    }

    private Tab currentTab = Tab.Create;

    // Variables for creating a new item
    private string newItemName = "";
    private string newItemDescription = "";
    private Sprite newItemIcon;
    private ItemSO.ItemType newItemType = ItemSO.ItemType.Misc;
    private bool newItemStackable = false;
    private int newItemMaxStackSize = 1;

    // Search and filter functionality
    private string searchQuery = "";
    private ItemSO.ItemType filterItemType = ItemSO.ItemType.Misc;
    private bool filterByType = false;
    private bool sortByAlphabet = true;

    public override void OnInspectorGUI()
    {
        ItemDatabaseSO database = (ItemDatabaseSO)target;

        // Create tabs
        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new string[] { "Create", "Edit", "Item List" });

        GUILayout.Space(10);

        // Switch between tabs
        switch (currentTab)
        {
            case Tab.Create:
                DrawCreateTab(database);
                break;

            case Tab.Edit:
                DrawEditTab(database);
                break;

            case Tab.ItemList:
                DrawItemListTab(database);
                break;
        }

        // Save changes to the database if anything has changed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(database);
        }
    }

    private void DrawCreateTab(ItemDatabaseSO database)
    {
        GUILayout.Label("Create New Item", EditorStyles.boldLabel);

        // Input fields for creating a new item
        newItemName = EditorGUILayout.TextField("Item Name", newItemName);

        // Automatically generate the next available ID
        int newItemId = GetNextAvailableId(database);
        EditorGUILayout.LabelField("Item ID", newItemId.ToString()); // Display the generated ID

        newItemDescription = EditorGUILayout.TextField("Description", newItemDescription);
        newItemIcon = (Sprite)EditorGUILayout.ObjectField("Icon", newItemIcon, typeof(Sprite), false);
        newItemType = (ItemSO.ItemType)EditorGUILayout.EnumPopup("Item Type", newItemType);
        newItemStackable = EditorGUILayout.Toggle("Is Stackable", newItemStackable);
        newItemMaxStackSize = EditorGUILayout.IntField("Max Stack Size", newItemMaxStackSize);

        // **New Field: Prefab Selection**
        GameObject newItemPrefab = (GameObject)EditorGUILayout.ObjectField("Item Prefab", null, typeof(GameObject), false);

        if (GUILayout.Button("Add Item"))
        {
            if (string.IsNullOrWhiteSpace(newItemName))
            {
                Debug.LogError("Item name cannot be empty!");
                return;
            }

            // Create a new item and assign properties
            ItemSO newItem = ScriptableObject.CreateInstance<ItemSO>();
            newItem.itemName = newItemName;
            newItem.itemId = newItemId.ToString(); // Use the auto-generated ID
            newItem.itemDescription = newItemDescription;
            newItem.itemIcon = newItemIcon;
            newItem.itemType = newItemType;
            newItem.isStackable = newItemStackable;
            newItem.maxStackSize = newItemMaxStackSize;
            newItem.itemPrefab = newItemPrefab; // Assign prefab reference

            // Ensure the directory exists
            string folderPath = "Assets/Items/";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Items");
            }

            // Save the new item as an asset
            string path = AssetDatabase.GenerateUniqueAssetPath(folderPath + newItemName + ".asset");
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.ImportAsset(path);

            // Add the new item to the database
            database.items.Add(newItem);
            EditorUtility.SetDirty(database);

            Debug.Log($"Item '{newItemName}' added to the database with ID '{newItemId}'.");

            // Reset input fields
            newItemName = "";
            newItemDescription = "";
            newItemIcon = null;
            newItemType = ItemSO.ItemType.Misc;
            newItemStackable = false;
            newItemMaxStackSize = 1;
        }
    }

    private void DrawEditTab(ItemDatabaseSO database)
    {
        GUILayout.Label("Edit Existing Items", EditorStyles.boldLabel);

        DrawSearchAndFilterControls();

        // Filter and sort items
        List<ItemSO> filteredItems = GetFilteredAndSortedItems(database);

        if (filteredItems.Count == 0)
        {
            GUILayout.Label("No items match your search or filter.");
            return;
        }

        // Display filtered items and allow editing
        foreach (var item in filteredItems)
        {
            EditorGUILayout.BeginVertical("box");

            item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
            item.itemId = EditorGUILayout.TextField("Item ID", item.itemId); // Allow editing of Item ID
            item.itemDescription = EditorGUILayout.TextField("Description", item.itemDescription);
            item.itemIcon = (Sprite)EditorGUILayout.ObjectField("Icon", item.itemIcon, typeof(Sprite), false);
            item.itemType = (ItemSO.ItemType)EditorGUILayout.EnumPopup("Item Type", item.itemType);
            item.isStackable = EditorGUILayout.Toggle("Is Stackable", item.isStackable);
            item.maxStackSize = EditorGUILayout.IntField("Max Stack Size", item.maxStackSize);

            // **New Prefab Field**
            item.itemPrefab = (GameObject)EditorGUILayout.ObjectField("Item Prefab", item.itemPrefab, typeof(GameObject), false);

            if (GUILayout.Button("Remove Item"))
            {
                database.items.Remove(item);
                EditorUtility.SetDirty(database);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(item));
                Debug.Log($"Item '{item.itemName}' removed from the database.");
                break;
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void DrawItemListTab(ItemDatabaseSO database)
    {
        GUILayout.Label("Item List", EditorStyles.boldLabel);

        DrawSearchAndFilterControls();

        // Filter and sort items
        List<ItemSO> filteredItems = GetFilteredAndSortedItems(database);

        if (filteredItems.Count == 0)
        {
            GUILayout.Label("No items match your search or filter.");
            return;
        }

        // Display filtered items
        foreach (var item in filteredItems)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.itemName, GUILayout.Width(200));
            GUILayout.Label($"ID: {item.itemId}", GUILayout.Width(100));
            GUILayout.Label($"Type: {item.itemType}");
            GUILayout.EndHorizontal();
        }
    }

    private void DrawSearchAndFilterControls()
    {
        // Search bar
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search", GUILayout.Width(50));
        searchQuery = GUILayout.TextField(searchQuery, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        // Filter by type
        GUILayout.BeginHorizontal();
        filterByType = EditorGUILayout.Toggle("Filter by Type", filterByType);
        if (filterByType)
        {
            filterItemType = (ItemSO.ItemType)EditorGUILayout.EnumPopup("Item Type", filterItemType);
        }
        GUILayout.EndHorizontal();

        // Sorting options
        GUILayout.BeginHorizontal();
        sortByAlphabet = EditorGUILayout.Toggle("Sort Alphabetically", sortByAlphabet);
        GUILayout.EndHorizontal();
    }

    private List<ItemSO> GetFilteredAndSortedItems(ItemDatabaseSO database)
    {
        IEnumerable<ItemSO> items = database.items;

        // Apply search query
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            items = items.Where(item => item.itemName.ToLower().Contains(searchQuery.ToLower()) || item.itemId.Contains(searchQuery));
        }

        // Apply type filter
        if (filterByType)
        {
            items = items.Where(item => item.itemType == filterItemType);
        }

        // Apply sorting
        items = sortByAlphabet
            ? items.OrderBy(item => item.itemName)
            : items.OrderBy(item => int.Parse(item.itemId));

        return items.ToList();
    }

    private int GetNextAvailableId(ItemDatabaseSO database)
    {
        HashSet<int> existingIds = new HashSet<int>();
        foreach (var item in database.items)
        {
            if (int.TryParse(item.itemId, out int id))
            {
                existingIds.Add(id);
            }
        }

        int nextId = 1;
        while (existingIds.Contains(nextId))
        {
            nextId++;
        }

        return nextId;
    }
}
