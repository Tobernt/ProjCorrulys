using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;

public class PlayerDataManager : NetworkBehaviour
{
    public string characterName;
    public int level;
    public float health;
    private Inventory inventory;

    private static string savePath => Path.Combine(Application.persistentDataPath, "Characters");

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        LoadCharacterData();

        // ✅ Ensure inventory is loaded after character data
        Inventory inventory = GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.LoadInventory();
            Debug.Log("✅ Inventory reloaded after local player initialization!");
        }
    }


    private void LoadCharacterData()
    {
        string selectedName = PlayerPrefs.GetString("SelectedCharacter", "DefaultPlayer");
        string filePath = Path.Combine(savePath, $"{selectedName}.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CharacterData data = JsonUtility.FromJson<CharacterData>(json);
            ApplyCharacterData(data);
        }
        else
        {
            Debug.LogError($"❌ Character file not found: {filePath}");
        }
    }

    private void ApplyCharacterData(CharacterData data)
    {
        if (data == null) return;

        characterName = data.Name;
        level = data.Level;
        health = data.Health;

        // ✅ Apply inventory data
        inventory = GetComponent<Inventory>();
        if (inventory != null)
        {
            Debug.Log("✅ Inventory Component Found in PlayerDataManager!");
            inventory.LoadInventory(); // ✅ Use LoadInventory() instead of LoadInventoryData()
        }
        else
        {
            Debug.LogError("❌ Inventory component not found on player in PlayerDataManager!");
        }

        Debug.Log($"✅ Loaded Character: {characterName}, Level: {level}, Health: {health}");
    }

    public void SaveCharacterData()
    {
        if (!isLocalPlayer) return;

        string filePath = Path.Combine(savePath, $"{characterName}.json");

        CharacterData data = new CharacterData(characterName, level, health);
        data.Inventory = new List<InventorySlot>(inventory.slots);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"💾 Character {characterName} saved.");
    }

    public void UpdateCharacterStats(int newLevel, float newHealth)
    {
        level = newLevel;
        health = newHealth;
        SaveCharacterData();
    }
}
