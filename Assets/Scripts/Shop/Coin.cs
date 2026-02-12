using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 30000;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoins(coinValue);
            Destroy(gameObject);
        }
    }
}

