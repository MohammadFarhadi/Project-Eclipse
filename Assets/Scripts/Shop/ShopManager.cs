using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private PlayerControllerBase currentPlayer;

    public ShopItem[] shopItems;  // آرایه آیتم‌های شاپ
    public static ShopManager Instance;
    public GameObject ShopUI;
    public void SetCurrentPlayer(PlayerControllerBase player)
    {
        currentPlayer = player;
    }

    // شماره آیتم را می‌گیر و برای پلیر خرید می‌کند
    public void BuyItem(int itemIndex)
    {
        if (currentPlayer == null)
        {
            Debug.Log("No player in shop!");
            return;
        }

        if (itemIndex < 0 || itemIndex >= shopItems.Length)
        {
            Debug.Log("Invalid item index!");
            return;
        }

        shopItems[itemIndex].BuyItem(currentPlayer);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // اختیاری
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (ShopUI == null)
        {
            ShopUI = GameObject.FindWithTag("ShopUI");
        }
    }

    public GameObject GetShopUI()
    {
        return ShopUI;
    }
}