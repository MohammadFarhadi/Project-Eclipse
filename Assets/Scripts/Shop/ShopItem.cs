using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public enum ItemType { Key, Health, Stamina ,Damage ,DamageResist }
    public ItemType itemType;

    public void BuyItem(PlayerControllerBase player)
    {
        if (player == null)
        {
            Debug.Log("No player provided for buying!");
            return;
        }

        int cost = 0;

        switch (itemType)
        {
            case ItemType.Key:
                cost = 100000;
                break;
            case ItemType.Health:
                cost = 30000;
                break;
            case ItemType.Stamina:
                cost = 20000;
                break;
            case ItemType.Damage:
                cost = 50000;
                break;
            case ItemType.DamageResist:
                cost = 50000;
                break;
        }

        if (CoinManager.Instance.SpendCoins(cost))
        {
            switch (itemType)
            {
                case ItemType.Key:
                    player.HasKey = true;
                    break;
                case ItemType.Health:
                    player.HealthSystem(100, true);
                    break;
                case ItemType.Stamina:
                    player.SetStamina(100);
                    break;
                case ItemType.Damage:
                    player.IncreaseMeleeDamageTemporarily(5);
                    break;
                case ItemType.DamageResist:
                    player.ActivateDamageResetItem();
                    break;
            }
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }
}