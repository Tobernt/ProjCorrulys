using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string Name;
    public int Level;
    public float Health;
    public List<InventorySlot> Inventory; // Now handles inventory!

    private static string savePath => Path.Combine(Application.persistentDataPath, "Characters");

    public CharacterData(string name, int level, float health)
    {
        this.Name = name;
        this.Level = level;
        this.Health = health;
        this.Inventory = new List<InventorySlot>();
    }

    // Save Character + Inventory in one file
    public void Save()
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(Path.Combine(savePath, $"{Name}.json"), json);
        Debug.Log($"💾 Character {Name} saved! Inventory Size: {Inventory.Count}");
    }

    // Load Character (Inventory included)
    public static CharacterData Load(string characterName)
    {
        string filePath = Path.Combine(savePath, $"{characterName}.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CharacterData data = JsonUtility.FromJson<CharacterData>(json);

            Debug.Log($"✅ Character {characterName} loaded! Inventory Size: {data.Inventory.Count}");
            return data;
        }

        Debug.LogError($"❌ Character file not found: {filePath}");
        return null;
    }

    // Delete Character
    public static void DeleteCharacter(string characterName)
    {
        string filePath = Path.Combine(savePath, $"{characterName}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"🗑️ Character {characterName} deleted!");
        }
    }

    // Get all saved characters
    public static List<CharacterData> GetAllCharacters()
    {
        List<CharacterData> characters = new List<CharacterData>();
        if (Directory.Exists(savePath))
        {
            foreach (string file in Directory.GetFiles(savePath, "*.json"))
            {
                string json = File.ReadAllText(file);
                CharacterData character = JsonUtility.FromJson<CharacterData>(json);
                characters.Add(character);
            }
        }
        return characters;
    }
}
