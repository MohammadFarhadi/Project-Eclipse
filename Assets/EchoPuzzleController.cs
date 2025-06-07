using UnityEngine;

public class EchoPuzzleController : MonoBehaviour
{
    public GameObject guardianBarrier; // barrier GameObject to deactivate later

    private Color[] correctSequence = {
        Color.blue, Color.magenta, Color.green
    };

    private int currentSequenceIndex = 0;

    public void OrbActivated(Color orbColor)
    {
        if (orbColor == correctSequence[currentSequenceIndex])
        {
            Debug.Log("Correct orb activated");
            currentSequenceIndex++;

            if (currentSequenceIndex >= correctSequence.Length)
                PuzzleComplete();
        }
        else
        {
            Debug.Log("Incorrect orb, resetting sequence");
            currentSequenceIndex = 0;
        }
    }

    void PuzzleComplete()
    {
        Debug.Log("Puzzle solved! Deactivating barrier.");
        guardianBarrier.SetActive(false);
        // We'll activate enemies later here as well.
    }
}