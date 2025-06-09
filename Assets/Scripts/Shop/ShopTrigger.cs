using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI;
    public ShopManager shopManager;

    private void Awake()
    {
        shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
        shopUI = ShopManager.Instance.GetShopUI();

        if (shopUI == null)
        {
            Debug.LogWarning("FadePanel is null in FadeManager.");
            return;
        }

        if (shopUI.GetComponent<CanvasGroup>() == null)
        {
            shopUI.AddComponent<CanvasGroup>();
        }

        shopUI.SetActive(false);
        /*
         * // مقداردهی خودکار به shopUI اگر null باشد
        if (shopUI == null)
        {
            shopUI = GameObject.Find("ShopCanvas");
            if (shopUI == null)
                Debug.LogWarning("ShopCanvas not found in the scene. Please check the name.");
        }

        // مقداردهی خودکار به shopManager اگر null باشد
        if (shopManager == null)
        {
            shopManager = FindObjectOfType<ShopManager>();
            if (shopManager == null)
                Debug.LogWarning("ShopManager not found in the scene.");
        }

        // غیرفعال کردن UI در شروع
        if (shopUI != null)
            shopUI.SetActive(false);
         */
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (shopUI != null) shopUI.SetActive(true);

            var player = other.GetComponent<PlayerControllerBase>();
            shopManager.SetCurrentPlayer(player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (shopUI != null) shopUI.SetActive(false);

            if (shopManager != null) shopManager.SetCurrentPlayer(null);
        }
    }
}