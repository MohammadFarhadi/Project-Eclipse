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

    private bool _isSkipping = false; // برای کنترل اسکیپ
    private Coroutine typingCoroutine;

    private void Start()
    {
        button.SetActive(false);
        typingCoroutine = StartCoroutine(ShowText());
    }

    private void Update()
    {
        // اگر بازیکن Space بزنه
        if (Input.GetKeyDown(KeyCode.Space) && !_isSkipping)
        {
            _isSkipping = true;
        }
    }

    IEnumerator ShowText()
    {
        textUI.text = ""; // ابتدا متن را پاک می‌کنیم

        for (int i = 0; i < fullText.Length; i++)
        {
            // اگر اسکیپ فعال شد، فوراً متن کامل رو نشون بده و حلقه رو بشکن
            if (_isSkipping)
            {
                textUI.text = fullText;
                break;
            }

            textUI.text += fullText[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        button.SetActive(true); 
    }

    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SelectionScene");
    }
}