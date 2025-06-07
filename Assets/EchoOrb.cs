using UnityEngine;

public class EchoOrb : MonoBehaviour
{
    public Color orbColor;
    private EchoPuzzleController puzzleController;
    private EchoDialogueTrigger dialogueTrigger;
    private bool playerInside = false;

    void Start()
    {
        puzzleController = FindObjectOfType<EchoPuzzleController>();
        dialogueTrigger = GetComponent<EchoDialogueTrigger>();
        GetComponent<SpriteRenderer>().color = orbColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            dialogueTrigger.ShowDialogue(); // نمایش دیالوگ
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            dialogueTrigger.HideDialogue(); // بستن دیالوگ
            puzzleController.OrbActivated(orbColor); // فعال شدن پازل بعد از خروج
        }
    }
}