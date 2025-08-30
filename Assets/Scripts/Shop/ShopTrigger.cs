using UnityEngine;
using Unity.Netcode; // اگر NGO استفاده میکنی

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI;
    private bool canOpenShop = false;
    private PlayerControllerBase localPlayer;

    public ShopManager shopManager;

    private void Awake()
    {
        shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
        //shopUI = ShopManager.Instance.GetShopUI();

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
        
    }

    private void Update()
    {
        // فقط روی لوکال پلیر منطق اجرا بشه
        if (localPlayer != null) // شرط برای لوکال پلیر
        {
            if (canOpenShop && Input.GetKeyDown(KeyCode.B))
            {
                ToggleShop();
            }
        }
    }

    private void ToggleShop()
    {
        shopUI.SetActive(!shopUI.activeSelf);
        shopManager.SetCurrentPlayer(localPlayer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerControllerBase>();
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (player != null && player.IsOwner) // فقط لوکال پلیر
            {
                canOpenShop = true;
                localPlayer = player;
            }
        }
        else
        {
            if (player != null)
            {
                canOpenShop = true;
                localPlayer = player;
            }
        }
       
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerControllerBase>();
        if (GameModeManager.Instance.CurrentMode == GameMode.Online)
        {
            if (player != null && player.IsOwner) // فقط لوکال پلیر
            {
                canOpenShop = false;
                shopUI.SetActive(false); // اگه بیرون رفت ببنده
                localPlayer = null;
            }
        }
        else
        {
            if (player != null)
            {
                canOpenShop = false;
                shopUI.SetActive(false); // اگه بیرون رفت ببنده
                localPlayer = null;
            }
        }
       
    }
}