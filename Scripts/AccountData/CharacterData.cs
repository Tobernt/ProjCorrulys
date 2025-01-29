using System;
using UnityEngine;

[Serializable]
public struct CharacterData
{
    public string characterId;  // Unique ID for the character
    public string characterName;
    public int level;
    public float health;
    public float stamina;
    public string inventoryData; // Serialized inventory JSON

    public CharacterData(string id, string name, int level, float health, float stamina)
    {
        characterId = id;
        characterName = name;
        this.level = level;
        this.health = health;
        this.stamina = stamina;
        inventoryData = "";
    }
}
