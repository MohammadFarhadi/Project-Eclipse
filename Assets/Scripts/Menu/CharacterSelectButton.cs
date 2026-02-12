using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    public int characterID; // 0 و 1 برای Melee, 2 و 3 برای Ranged

    private Button button;
    private Image buttonImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
    }

    public void SetColor(Color color)
    {
        buttonImage.color = color;
    }

    public void ResetColor()
    {
        buttonImage.color = Color.white;
    }
}