using Unity.Netcode;
using UnityEngine;

public class CollectibleItem : NetworkBehaviour
{
    public enum CollectibleType { Health, Stamina , Key}

    public CollectibleType type;
    public int value = 10; // مقدار افزایش

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
        if (player != null)
        {
            switch (type)
            {
                case CollectibleType.Health:
                    player.HealthSystem(100, true); // جون اضافه کن
                    break;
                case CollectibleType.Stamina:
                    //اینجا
                    player.SetStamina(50f); // استامینا اضافه کن
                    // تا اینجا
                    break;
                case CollectibleType.Key:
                    player.HasKey = true;
                    break;
            }

            Destroy(gameObject); // آیتم از بین بره بعد از جمع‌آوری
        }
    }
}