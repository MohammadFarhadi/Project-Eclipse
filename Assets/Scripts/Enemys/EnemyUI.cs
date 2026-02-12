using UnityEngine;
using UnityEngine.UI;

namespace Enemys
{
    public class EnemyUI : MonoBehaviour
    {
        [Header("Bar")]
        public Slider healthBar;
     
        public void SetHealthBar(float current, float max)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
        
    }
}