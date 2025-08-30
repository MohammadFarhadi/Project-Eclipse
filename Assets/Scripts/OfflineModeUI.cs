using UnityEngine;

public class OfflineModeUI : MonoBehaviour
{
    public GameObject offlineModeUI;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activateOfflineModeUI()
    {
        offlineModeUI.SetActive(true);
    }

    public void deactivateOfflineModeUI()
    {
        offlineModeUI.SetActive(false);
    }
}
