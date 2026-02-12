using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalCharacterSelectionManager : MonoBehaviour
{
    public static LocalCharacterSelectionManager Instance;
    public Button ConfirmButton;
    private int[] selectedCharacters = new int[2] { -1, -1 };
    [SerializeField] private CharacterSelectButton[] characterButtons;

    private int currentPlayerIndex = 0; // بازیکن اول شروع می‌کند

    private Color[] playerColors = new Color[2] { Color.red, Color.blue };

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void SelectCharacter(int characterID)
    {
        // قانون: فقط یک Melee و یک Ranged انتخاب می‌شود
        if (!IsValidSelection(characterID, currentPlayerIndex)) return;

        selectedCharacters[currentPlayerIndex] = characterID;
        UpdateButtonColors();
        currentPlayerIndex = 1 - currentPlayerIndex;
        Debug.Log($"Player {currentPlayerIndex + 1} turn to select");
    }

    private bool IsValidSelection(int characterID, int playerIndex)
    {
        if(playerIndex == 1 && selectedCharacters[0] != -1)
        {
            bool firstIsMelee = selectedCharacters[0] < 2;
            bool secondIsMelee = characterID < 2;
            if(firstIsMelee == secondIsMelee) return false;
        }
        return true;
    }

    private void UpdateButtonColors()
    {
        foreach(var btn in characterButtons)
            btn.ResetColor();

        for(int i=0; i<2; i++)
        {
            int c = selectedCharacters[i];
            if(c != -1)
                characterButtons[c].SetColor(playerColors[i]);
        }
    }
    public void ConfirmSelection()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local && selectedCharacters[0] != -1 && selectedCharacters[1] != -1)
        {
            SceneManager.LoadScene("Level1");
        }
    }
    public int[] GetSelectedCharacters()
    {
        return selectedCharacters;
    }

}