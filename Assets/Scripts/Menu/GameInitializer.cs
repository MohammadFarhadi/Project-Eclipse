using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Ranged Players")]
    [SerializeField] private GameObject rangedPlayer1;
    [SerializeField] private GameObject rangedPlayer2;

    [Header("Melee Players")]
    [SerializeField] private GameObject meleePlayer1;
    [SerializeField] private GameObject meleePlayer2;

    void Start()
    {
        // only declare rangedIndex once
        int rangedIndex = PlayerSelectionData.Instance.selectedRangedIndex;
        int meleeIndex  = PlayerSelectionData.Instance.selectedMeleeIndex;

        // ابتدا همه پلیرها غیرفعال می‌شن
        rangedPlayer1.SetActive(false);
        rangedPlayer2.SetActive(false);
        meleePlayer1.SetActive(false);
        meleePlayer2.SetActive(false);

        // فعال‌سازی پلیر انتخاب شده و به‌روزرسانی UI
        if (rangedIndex == 1)
        {
            rangedPlayer1.SetActive(true);
            rangedPlayer1.GetComponent<PlayerControllerBase>().RefreshUI();
        }
        else if (rangedIndex == 2)
        {
            rangedPlayer2.SetActive(true);
            rangedPlayer2.GetComponent<PlayerControllerBase>().RefreshUI();
        }

        if (meleeIndex == 1)
        {
            meleePlayer1.SetActive(true);
            meleePlayer1.GetComponent<PlayerControllerBase>().RefreshUI();
        }
        else if (meleeIndex == 2)
        {
            meleePlayer2.SetActive(true);
            meleePlayer2.GetComponent<PlayerControllerBase>().RefreshUI();
        }

        // اینجا می‌تونی تنظیمات مربوط به دوربین یا هر چیز دیگه‌ای رو هم انجام بدی
    }
}