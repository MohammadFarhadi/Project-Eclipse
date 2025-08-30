using UnityEngine;

public class DestroybleObject : MonoBehaviour
{
    public int hp = 50;
    private void onCollisionEnter2D(Collision2D collision)
    {
        var melee = collision.gameObject.GetComponent<MeleePlayerController>();
        if (melee != null)
        {
            TakeDamage(melee.GetAttackDamage());
        }
        var ranged = collision.gameObject.GetComponent<RangedPlayerController>();
        if (ranged != null)
        {
            TakeDamage(ranged.GetAttackDamage());
        }
        
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
