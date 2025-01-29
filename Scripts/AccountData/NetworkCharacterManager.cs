using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkCharacterManager : NetworkBehaviour
{
    public static NetworkCharacterManager Instance { get; private set; }

    public readonly SyncList<CharacterData> serverCharacters = new SyncList<CharacterData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnStartServer()
    {
        Debug.Log("NetworkCharacterManager started on the server.");
    }

    public override void OnStartClient()
    {
        Debug.Log("NetworkCharacterManager started on the client.");
    }
    public CharacterData GetCharacterForPlayer(NetworkConnectionToClient sender)
    {
        if (playerSelectedCharacters.ContainsKey(sender))
        {
            return playerSelectedCharacters[sender];
        }
        return new CharacterData(); // Return default (empty) if not found
    }

    private Dictionary<NetworkConnectionToClient, CharacterData> playerSelectedCharacters = new Dictionary<NetworkConnectionToClient, CharacterData>();

    [Command]
    public void CmdSelectCharacter(CharacterData character, NetworkConnectionToClient sender = null)
    {
        if (playerSelectedCharacters.ContainsKey(sender))
            playerSelectedCharacters[sender] = character;
        else
            playerSelectedCharacters.Add(sender, character);

        RpcConfirmCharacterSelection(character);
    }


    // ClientRpc: Notify all clients about the selection
    [ClientRpc]
    private void RpcConfirmCharacterSelection(CharacterData character)
    {
        Debug.Log($"Character {character.characterName} confirmed for play.");
        CharacterSelectionUI.Instance.ConfirmCharacterSelection(character);
    }
}
