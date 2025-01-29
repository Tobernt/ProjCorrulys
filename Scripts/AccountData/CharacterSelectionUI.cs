using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    public static CharacterSelectionUI Instance { get; private set; }

    public CharacterStorage characterStorage;
    public Transform characterListContainer;
    public GameObject characterSlotPrefab;
    public Button loadCharacterButton;

    private CharacterData selectedCharacter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        PopulateCharacterList();
        loadCharacterButton.onClick.AddListener(LoadSelectedCharacter);
    }

    public void PopulateCharacterList()
    {
        foreach (Transform child in characterListContainer) Destroy(child.gameObject);

        List<CharacterData> characters = characterStorage.GetCharacters();

        foreach (CharacterData character in characters)
        {
            GameObject slot = Instantiate(characterSlotPrefab, characterListContainer);
            slot.GetComponentInChildren<Text>().text = $"{character.characterName} (Level {character.level})";

            Button button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => SelectCharacter(character));
        }
    }

    private void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        Debug.Log($"Selected character: {character.characterName}");
    }

    public void LoadSelectedCharacter()
    {
        if (!string.IsNullOrEmpty(selectedCharacter.characterId))
        {
            Debug.Log($"Loading {selectedCharacter.characterName}...");
            NetworkCharacterManager.Instance.CmdSelectCharacter(selectedCharacter);
        }
        else
        {
            Debug.LogWarning("No character selected!");
        }
    }

    public void ConfirmCharacterSelection(CharacterData character)
    {
        Debug.Log($"Character {character.characterName} loaded successfully!");
        // Transition to the game scene or apply character data
    }
}
