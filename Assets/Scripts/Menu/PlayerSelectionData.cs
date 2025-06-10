using UnityEngine;

public class PlayerSelectionData : MonoBehaviour
{
    public static PlayerSelectionData Instance;

    public int selectedRangedIndex = 0;
    public int selectedMeleeIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // حفظ بین صحنه‌ها
        }
        else
        {
            Destroy(gameObject);
        }
    }
}