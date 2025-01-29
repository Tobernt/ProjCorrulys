using UnityEngine;
using Mirror;

public class CharacterLoader : NetworkBehaviour
{
    public string characterName;
    public int level;
    public float health;
    public float stamina;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdRequestCharacterData();
    }

    // Ask the server for our selected character data
    [Command]
    private void CmdRequestCharacterData(NetworkConnectionToClient sender = null)
    {
        CharacterData selectedCharacter = NetworkCharacterManager.Instance.GetCharacterForPlayer(sender);
        if (!string.IsNullOrEmpty(selectedCharacter.characterId))
        {
            RpcApplyCharacterData(selectedCharacter);
        }
    }

    // Apply character data on all clients
    [ClientRpc]
    private void RpcApplyCharacterData(CharacterData character)
    {
        characterName = character.characterName;
        level = character.level;
        health = character.health;
        stamina = character.stamina;

        Debug.Log($"Character {characterName} loaded with Level {level}, Health {health}, Stamina {stamina}");
    }
}
