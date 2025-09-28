using UnityEngine;
using UnityEngine.UI;
public class OnlineController : MonoBehaviour
{
    public GameObject panel;       // پنل مورد نظر
    public Button openButton;      // دکمه‌ای که پنل رو باز می‌کنه
    public Button closeButton;     // دکمه‌ای که پنل رو می‌بنده

    void Start()
    {
        panel.SetActive(false); // از اول پنل غیرفعال باشه

        // وصل کردن دکمه‌ها به توابع
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);
    }

    void OpenPanel()
    {
        GameModeManager.Instance.CurrentMode = GameMode.Online;
        panel.GetComponent<UIAnimator>().Show(); // باز کردن پنل
    }

    void ClosePanel()
    {
        panel.GetComponent<UIAnimator>().Hide(); // بستن پنل
    }
}

