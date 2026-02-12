using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("پنل‌ها")]
    public GameObject mainPanel;    // پنل اصلی که دکمه‌ها را دارد
    public GameObject optionPanel;
    public GameObject creditPanel;

    private GameObject currentPanel;

    // نمایش Option
    public void OnOptionClicked()
    {
        SwitchPanel(optionPanel);
    }

    // نمایش Credit
    public void OnCreditClicked()
    {
        SwitchPanel(creditPanel);
    }

    // بازگشت به پنل اصلی
    public void OnBackClicked()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    private void SwitchPanel(GameObject targetPanel)
    {
        // ابتدا پنل اصلی را Hide کن
        if (mainPanel != null)
            mainPanel.SetActive(false);

        // پنل قبلی را Hide کن
        if (currentPanel != null)
            currentPanel.SetActive(false);

        // پنل جدید را Show کن
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            currentPanel = targetPanel;
        }
    }
}