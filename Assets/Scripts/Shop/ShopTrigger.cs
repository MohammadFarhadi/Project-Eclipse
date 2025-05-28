using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI;
    public ShopManager shopManager;  // ارجاع به اسکریپت ShopManager

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.SetActive(true);
            var player = other.GetComponent<PlayerControllerBase>();
            shopManager.SetCurrentPlayer(player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.SetActive(false);
            shopManager.SetCurrentPlayer(null);
        }
    }
}