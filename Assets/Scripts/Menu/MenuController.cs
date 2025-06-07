using UnityEngine;
using UnityEngine.SceneManagement; // ➕ مهم برای کار با Sceneها

public class MenuController : MonoBehaviour
{
    public GameObject homeValue;
    public GameObject profileValue;
    public GameObject optionValue;
    public GameObject quitValue;

    void Start()
    {
        optionValue.SetActive(false);
    }
    public void ShowHome()
    {
        DeactivateAll();
        homeValue.SetActive(true);
    }

    public void ShowProfile()
    {
        DeactivateAll();
        profileValue.SetActive(true);
    }

    public void ShowOption()
    {
        DeactivateAll();
        optionValue.SetActive(true);
    }

    public void ShowQuit()
    {
        DeactivateAll();
        quitValue.SetActive(true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // 👈 اسم دقیق Scene رو بذار اینجا
    }

    private void DeactivateAll()
    {
        homeValue.SetActive(false);
        profileValue.SetActive(false);
        optionValue.SetActive(false);
        quitValue.SetActive(false);
    }
}