using UnityEngine;

public class EnemyHealthBarDisplay : MonoBehaviour
{
    public Sprite[] healthSprites; // اسپرایت‌ها برای 3، 2، 1، 0
    public SpriteRenderer spriteRenderer; // SpriteRenderer خود HealthBar

    public void UpdateHealthBar(int health)
    {
        int index = Mathf.Clamp(health, 0, healthSprites.Length - 1);
        spriteRenderer.sprite = healthSprites[index];
    }
    public void Show(int health)
    {
        gameObject.SetActive(true);
        UpdateHealthBar(health);
    }

}    