using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButtonClick : MonoBehaviour
{
    private CharacterSelectButton characterSelectButton;

    private void Awake()
    {
        characterSelectButton = GetComponent<CharacterSelectButton>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if(GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            LocalCharacterSelectionManager.Instance.SelectCharacter(characterSelectButton.characterID);
        }
        else
        {
            
            OnlineCharacterSelectionManager.Instance.SelectCharacter(characterSelectButton.characterID);
        }
    }
}