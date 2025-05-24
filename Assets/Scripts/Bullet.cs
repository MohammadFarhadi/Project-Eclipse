
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //اینجا رو اضافه کردم
    private Transform attacker;

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
            other.GetComponent<InterfaceEnemies>().TakeDamage(1, attacker);
            
            

            gameObject.SetActive(false); // غیرفعال کردن گلوله به‌جای حذف
        }
        else if (other.CompareTag("Player"))
        {
            
            player.HealthSystem(10, false);
            Debug.Log("player hit bullet");
            gameObject.SetActive(false); // تیر پلیر هم بخوره، بره تو pool
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