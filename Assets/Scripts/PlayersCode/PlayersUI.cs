using UnityEngine;
using UnityEngine.UI;

public class PlayersUI : MonoBehaviour
{   
    [Header("Bar")]
    public Slider healthBar;

    [Header("Hearts")]
    public GameObject[] hearts; // 3 heart images

    public void SetHealthBar(float current, float max)
    {
        healthBar.maxValue = max;
        healthBar.value = current;
    }
    
}
