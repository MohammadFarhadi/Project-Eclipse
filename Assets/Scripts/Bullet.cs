
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //اینجا رو اضافه کردم
    private Transform attacker;
    public int damage = 1;
    public void SetAttacker(Transform attacker)
    {
        this.attacker = attacker;
    }
    // تا اینجا
    
    PlayerControllerBase player;
    private void Awake()
    {
        player = FindFirstObjectByType<PlayerControllerBase>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // فقط به Enemy آسیب بزن، نه به میدان دید
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<InterfaceEnemies>().TakeDamage(damage, attacker);
            gameObject.SetActive(false); // غیرفعال کردن گلوله به‌جای حذف
        }
        else if (other.CompareTag("Player"))
        {
            

            var targetPlayer = other.GetComponent<PlayerControllerBase>();
            if (targetPlayer != null)
            {
                targetPlayer.HealthSystem(30, false);
                Debug.Log($"{targetPlayer.name} got hit by bullet from {attacker?.name}");
            }

            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
        }

    }
    private void OnEnable()
    {
        // هر بار که فعال شد، بعد از ۱۰ ثانیه خودش خاموش بشه
        Invoke(nameof(Disable), 10f);
    }

    private void OnDisable()
    {
        CancelInvoke(); // اگه تیر زودتر غیرفعال شد، تایمر قطع شه
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}