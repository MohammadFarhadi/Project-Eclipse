using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotsUI : MonoBehaviour
{
    [Header("Rows 1..5 in order")]
    public SlotRow[] rows = new SlotRow[5];

    [Header("Scene to start for NEW games")]
    public string newGameSceneName = "CutsceneOrIntro";  // set in Inspector

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            var row  = rows[i];
            int slot = row.slotNumber;

            var meta   = SaveSystem.TryReadMeta(slot);
            bool exist = SaveSystem.Exists(slot);

            // --- Empty slot -> New Game ---
            if (!exist || meta == null)
            {
                if (row.title)   row.title.text   = $"Slot {slot}:";
                if (row.details) row.details.text = "Empty   Click to start a new game";

                if (row.primaryButton)
                {
                    row.primaryButton.onClick.RemoveAllListeners();
                    row.primaryButton.onClick.AddListener(() => StartNew(slot));
                }

                if (row.deleteButton) row.deleteButton.gameObject.SetActive(false);
            }
            // --- Used slot -> Load ---
            else
            {
                if (row.title)   row.title.text   = $"Slot {slot}";
                if (row.details) row.details.text =
                    $"Scene: {meta.scene} | Saved: {meta.savedAtLocal}\n" +
                    $"Player1 HP: {meta.p1hp:0}   Player2 HP: {meta.p2hp:0}";

                if (row.primaryButton)
                {
                    row.primaryButton.onClick.RemoveAllListeners();
                    row.primaryButton.onClick.AddListener(() => LoadSlot(slot));
                }

                if (row.deleteButton)
                {
                    row.deleteButton.gameObject.SetActive(true);
                    row.deleteButton.onClick.RemoveAllListeners();
                    row.deleteButton.onClick.AddListener(() =>
                    {
                        SaveSystem.DeleteSlot(slot);
                        Refresh();
                    });
                }
            }
        }
    }

    void StartNew(int slot)
    {
        SaveSystem.BeginSessionInSlot(slot); 
        if (!string.IsNullOrEmpty(newGameSceneName))
            SceneManager.LoadScene(newGameSceneName);   
        else
            Debug.LogError("[SaveSlotsUI] newGameSceneName not set.");
    }

    void LoadSlot(int slot)
    {
        SaveSystem.BeginSessionInSlot(slot);
        SaveSystem.StartRestore(slot); // NOT StartCoroutine

        SaveSystem.BeginSessionInSlot(slot);
    }

    
}
