using System;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("پنل‌ها")]
    public GameObject mainMenuPanel;    // پنل اصلی
    public GameObject onlinePanel;      // پنل Online Mode
    public GameObject localPanel;       // پنل Local Mode
    public GameObject optionPanel;      // پنل Option
    public GameObject creditPanel;      // پنل Credit

    private GameObject currentPanel;    // پنل فعلی که نمایش داده می‌شود

    
    // نمایش پنل Online
    public void OnOnlineModeClicked()
    {
        SwitchPanel(onlinePanel);
        GameModeManager.Instance.CurrentMode = GameMode.Online;
    }

    // نمایش پنل Local
    public void OnLocalModeClicked()
    {
        SwitchPanel(localPanel);
        GameModeManager.Instance.CurrentMode = GameMode.Local;
    }

    // نمایش پنل Option
    public void OnOptionClicked()
    {
        SwitchPanel(optionPanel);
    }

    // نمایش پنل Credit
    public void OnCreditClicked()
    {
        SwitchPanel(creditPanel);
    }

    // خروج از برنامه
    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // برای تست داخل Editor
#endif
    }

    // متد بازگشت به منوی اصلی
    public void OnBackClicked()
    {
        if (currentPanel != null)
        {
            currentPanel.GetComponent<UIAnimator>().Hide(); // فقط پنل فعلی بسته شود
            currentPanel = null;
        }

        mainMenuPanel.GetComponent<UIAnimator>().Show(); // نمایش منوی اصلی
    }

    // متد کمکی برای تغییر پنل‌ها
    private void SwitchPanel(GameObject targetPanel)
    {
        mainMenuPanel.GetComponent<UIAnimator>().Hide();

        if (currentPanel != null)
        {
            currentPanel.GetComponent<UIAnimator>().Hide();
        }

        targetPanel.GetComponent<UIAnimator>().Show();
        currentPanel = targetPanel;
    }
}