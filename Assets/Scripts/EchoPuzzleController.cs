using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoPuzzleController : MonoBehaviour
{
    [Tooltip("Your three orbs in their starting order (must match Slot0 → Slot1 → Slot2)")]
    public List<EchoOrb> orbList;

    [Tooltip("Empty GameObject slots that sit at the three world-positions")]
    public List<Transform> positions;

    [Tooltip("The correct target sequence")]
    public List<EchoOrb> correctOrder;

    [Tooltip("How long (in seconds) the swap animation should take")]
    public float swapDuration = 0.4f;

    private EchoOrb currentOrb = null;
    private bool isSwapping = false;

    void Update()
    {
        if (!isSwapping && currentOrb != null && Input.GetKeyDown(KeyCode.X))
        {
            int i = orbList.IndexOf(currentOrb);
            if (i >= 0)
            {
                int j = (i < orbList.Count - 1) ? i + 1 : i - 1;
                StartCoroutine(AnimateSwap(i, j));
            }
        }
    }

    public void SetCurrentOrb(EchoOrb orb)
    {
        currentOrb = orb;
    }

    private IEnumerator AnimateSwap(int i, int j)
    {
        isSwapping = true;

        // 1) grab orbs and their start/target positions
        var orbA = orbList[i];
        var orbB = orbList[j];
        Vector3 startA = orbA.transform.position;
        Vector3 startB = orbB.transform.position;
        Vector3 targetA = positions[j].position;
        Vector3 targetB = positions[i].position;

        // 2) temporarily un-parent so worldPositionStays
        orbA.transform.SetParent(null, worldPositionStays: true);
        orbB.transform.SetParent(null, worldPositionStays: true);

        // 3) animate LERP
        float t = 0f;
        while (t < swapDuration)
        {
            t += Time.deltaTime;
            float f = t / swapDuration;
            orbA.transform.position = Vector3.Lerp(startA, targetA, f);
            orbB.transform.position = Vector3.Lerp(startB, targetB, f);
            yield return null;
        }

        // 4) snap to final
        orbA.transform.position = targetA;
        orbB.transform.position = targetB;

        // 5) swap list order
        orbList[i] = orbB;
        orbList[j] = orbA;

        // 6) re-parent under the correct slot
        orbA.transform.SetParent(positions[j], worldPositionStays: false);
        orbB.transform.SetParent(positions[i], worldPositionStays: false);

        // 7) check for solved state
        CheckCorrectness();

        isSwapping = false;
    }
    public GameObject guardianBarrier;  // Assign this in inspector

    private void CheckCorrectness()
    {
        bool allCorrect = true;

        for (int k = 0; k < orbList.Count; k++)
        {
            bool correct = orbList[k] == correctOrder[k];
            orbList[k].SetSolvedState(correct);
            if (!correct) allCorrect = false;
        }

        if (allCorrect)
        {
            Debug.Log("Puzzle solved! Deactivating guardian barrier.");
            if (guardianBarrier != null)
                guardianBarrier.SetActive(false);
        }
    }

}
