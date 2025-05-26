using UnityEngine;

public class CollectibleItem : MonoBehaviour
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
                    player.HealthSystem(value, true); // جون اضافه کن
                    break;
                case CollectibleType.Stamina:
                    player.StaminaSystem(value, true); // استامینا اضافه کن
                    break;
                case CollectibleType.Key:
                    player.HasKey = true;
                    break;
            }

            Destroy(gameObject); // آیتم از بین بره بعد از جمع‌آوری
        }
    }
}