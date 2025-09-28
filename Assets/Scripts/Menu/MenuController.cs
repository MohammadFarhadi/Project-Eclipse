using UnityEngine;
using UnityEngine.SceneManagement;
using UintInspector = Unity.VisualScripting.UintInspector; // ➕ مهم برای کار با Sceneها

public class MenuController : MonoBehaviour
{
    public GameObject homeValue;
    public GameObject optionValue;

    void Start()
    {
        optionValue.SetActive(false);
    }
    public void ShowHome()
    {
        DeactivateAll();
        homeValue.GetComponent<UIAnimator>().Show();
    }

  

    public void ShowOption()
    {
        DeactivateAll();
        optionValue.GetComponent<UIAnimator>().Show();
    }

    

    public void PlayGame()
    {
        GameModeManager.Instance.CurrentMode = GameMode.Local;
        SceneManager.LoadScene("Starting Scene"); // 👈 اسم دقیق Scene رو بذار اینجا
    }

    private void DeactivateAll()
    {
        homeValue.SetActive(false);
        optionValue.SetActive(false);
    }
}