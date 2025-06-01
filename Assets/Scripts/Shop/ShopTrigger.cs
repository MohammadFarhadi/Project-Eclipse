using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopUI;
    public ShopManager shopManager;  // ارجاع به اسکریپت ShopManager]
    PlayerControllerBase currentPlayer;


    private void OnTriggerEnter2D(Collider2D other)
    {        
        PlayerControllerBase player1 = other.GetComponent<PlayerControllerBase>();
        if (player1 != null)
        {
            currentPlayer = player1;
        }
        
        if (other.CompareTag("Player") && currentPlayer.IsInteracting())
        {
            shopUI.SetActive(true);
            var player = other.GetComponent<PlayerControllerBase>();
            shopManager.SetCurrentPlayer(player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || currentPlayer.IsInteracting())
        {
            shopUI.SetActive(false);
            shopManager.SetCurrentPlayer(null);
        }
    }
}