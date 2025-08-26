using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private float cooldownTime = 2f;  // seconds between autosaves at this checkpoint
    private float _lastSaveTime = -999f;
    private bool  _savedThisEntry = false;

    private RangedPlayerController GetActiveRanged()
    {
        foreach (var ranged in FindObjectsOfType<RangedPlayerController>())
            if (ranged.gameObject.activeInHierarchy)
            {
                return ranged;
            }
        return null;
    }

    private MeleePlayerController GetActiveMelee()
    {
        foreach (var melee in FindObjectsOfType<MeleePlayerController>())
            if (melee.gameObject.activeInHierarchy)
            {
                return melee;
            }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _savedThisEntry = false;   // allow one save per entry
        TryAutoSave();
    }

    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     if (!other.CompareTag("Player")) return;
    // }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        _savedThisEntry = false;   // next entry can save again
    }

    private void TryAutoSave()
    {
        if (_savedThisEntry)
        {
            return;
        }

        if (Time.time - _lastSaveTime < cooldownTime)
        {
            return;
        }

        int slot = SaveSystem.GetActiveSlot();
        if (slot < 1 || slot > 5)
        {
            Debug.LogError("[Checkpoint] Autosave aborted: active slot not set (got " + slot + ").");
            return;
        }

        var melee  = GetActiveMelee();
        var ranged = GetActiveRanged();

        if (melee == null || ranged == null)
        {
            Debug.LogError("[Checkpoint] Autosave aborted: active melee or ranged is null.");
            return;
        }

        SaveSystem.AutoSave(melee, ranged);

        _savedThisEntry = true;
        _lastSaveTime   = Time.time;

        Debug.Log("[Checkpoint] Autosaved to slot " + slot + " at checkpoint '" + name + "'. Path: " + Application.persistentDataPath);
    }
}
