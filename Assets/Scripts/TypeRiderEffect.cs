using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textUI;   // متن مورد نظر
    [TextArea] public string fullText;   // متن کامل برای نمایش
    public float typingSpeed = 0.05f;    // سرعت تایپ (ثانیه بین هر حرف)
    public GameObject button;
    private void Start()
    {
        button.SetActive(false);
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        textUI.text = ""; // ابتدا متن را پاک می‌کنیم

        for (int i = 0; i < fullText.Length; i++)
        {
            textUI.text += fullText[i];
            yield return new WaitForSeconds(typingSpeed);
        }
        button.SetActive(true); 
    }
    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
