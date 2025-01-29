using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;

public class CharacterStorage : NetworkBehaviour
{
    private static string localSavePath => Path.Combine(Application.persistentDataPath, "Characters.json");

    public bool useServerStorage = false; // Toggle between local and server storage

    public List<CharacterData> localCharacters = new List<CharacterData>(); // Local storage

    // ✅ Use a **SyncListCharacterData** instead of a List<CharacterData>
    public class SyncListCharacterData : SyncList<CharacterData> { }
    public readonly SyncListCharacterData serverCharacters = new SyncListCharacterData(); // Server storage

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("CharacterStorage started on server.");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("CharacterStorage started on client.");
    }

    // Save a new character (either locally or on the server)
    public void SaveCharacter(CharacterData character)
    {
        if (useServerStorage)
        {
            CmdSaveCharacterToServer(character);
        }
        else
        {
            SaveCharacterLocally(character);
        }
    }

    // Save character locally
    private void SaveCharacterLocally(CharacterData character)
    {
        LoadLocalCharacters(); // Ensure we have the latest data
        localCharacters.Add(character);
        File.WriteAllText(localSavePath, JsonUtility.ToJson(new CharacterListWrapper(localCharacters), true));
        Debug.Log($"Saved {character.characterName} locally.");
    }

    // Load characters from local storage
    public void LoadLocalCharacters()
    {
        if (File.Exists(localSavePath))
        {
            string json = File.ReadAllText(localSavePath);
            localCharacters = JsonUtility.FromJson<CharacterListWrapper>(json).characters;
            Debug.Log("Loaded local characters.");
        }
    }

    // Server-side save (Mirror Command)
    [Command]
    private void CmdSaveCharacterToServer(CharacterData character)
    {
        serverCharacters.Add(character);
        RpcUpdateServerCharacters();
        Debug.Log($"Saved {character.characterName} on the server.");
    }

    // Sync characters for clients
    [ClientRpc]
    private void RpcUpdateServerCharacters()
    {
        Debug.Log("Server character list updated.");
    }

    // Get characters list (local or server)
    public List<CharacterData> GetCharacters()
    {
        return useServerStorage ? new List<CharacterData>(serverCharacters) : localCharacters;
    }

    // Wrapper for JSON serialization of character list
    [System.Serializable]
    private class CharacterListWrapper
    {
        public List<CharacterData> characters;
        public CharacterListWrapper(List<CharacterData> characters) => this.characters = characters;
    }
}
