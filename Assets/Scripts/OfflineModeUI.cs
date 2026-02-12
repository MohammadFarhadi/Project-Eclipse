using UnityEngine;

public class OfflineModeUI : MonoBehaviour
{
    public GameObject offlineModeUI;
    public GameObject credit;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activateOfflineModeUI()
    {
        offlineModeUI.GetComponent<UIAnimator>().Show();
        credit.SetActive(false);
    }

    public void deactivateOfflineModeUI()
    {
        offlineModeUI.GetComponent<UIAnimator>().Hide();
    }
}
