using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadinLvl2 : NetworkBehaviour
{
    public Animator doorAnimator;
    private bool player1Inside = false;
    private bool player2Inside = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "RangedPlayer(Clone)" || other.gameObject.name == "Ranged1Player(Clone)")
            player1Inside = true;

        if (other.gameObject.name == "Melle1Player(Clone)" || other.gameObject.name == "Melle2Player(Clone)")
            player2Inside = true;

        CheckIfBothPlayersInside();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "RangedPlayer(Clone)" || other.gameObject.name == "Ranged1Player(Clone)")
            player1Inside = false;

        if (other.gameObject.name == "Melle1Player(Clone)" || other.gameObject.name == "Melle2Player(Clone)")
            player2Inside = false;
    }

    void CheckIfBothPlayersInside()
    {
        if (player1Inside && player2Inside)
        {
            Debug.Log("Both players inside and door is open!");

            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                // برای حالت Local، می‌تونی مستقیم سکانس رو بارگذاری کنی
                SceneManager.LoadScene("Level2");
            }
            else
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                    {
                        var playerObj = client.PlayerObject;
                        if (playerObj != null)
                        {
                            playerObj.Despawn(true); // true برای destroy شدن کامل
                        }
                    }
                    var bulletPool = FindObjectOfType<BulletPoolNGO>();
                    if (bulletPool != null)
                    {
                        bulletPool.DespawnAllBullets();
                    }
                    // فقط سرور سکانس رو بارگذاری کنه و اجازه بده Netcode خودش کلاینت‌ها رو منتقل کنه
                    NetworkManager.Singleton.SceneManager.LoadScene("Level2", LoadSceneMode.Single);
                }
            }
        }
    }
}