using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationUI : MonoBehaviour
{
    public CharacterStorage characterStorage;
    public InputField characterNameInput;
    public Button createCharacterButton;

    private void Start()
    {
        createCharacterButton.onClick.AddListener(CreateCharacter);
    }

    private void CreateCharacter()
    {
        string name = characterNameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Character name cannot be empty!");
            return;
        }

        string uniqueId = System.Guid.NewGuid().ToString(); // Generate a unique ID
        CharacterData newCharacter = new CharacterData(uniqueId, name, 1, 100, 100);
        characterStorage.SaveCharacter(newCharacter);

        Debug.Log($"Character {name} created!");
    }
}
