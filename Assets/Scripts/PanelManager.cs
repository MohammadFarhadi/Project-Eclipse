using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // اگر دوست داشتی بین صحنه‌ها بمونه
        }
        else
        {
            Destroy(gameObject);
        }

        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);
    }
}