using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    public int currentCoins = 0;

    public TMP_Text coinText;  // متن UI TextMeshPro برای نمایش تعداد سکه‌ها

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateCoinText();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinText();
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            UpdateCoinText();
            return true;
        }
        return false;
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coins: " + currentCoins.ToString();
    }
}