using UnityEngine;
using UnityEngine.Tilemaps;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject dialoguePanel;           // کل پنل دیالوگ
    public DialogueTree dialogueTreeScript;    // اسکریپت درخت دیالوگ
    public GameObject player1; // پلیر برای خاموش کردن حرکت
    public GameObject player2;
    

    private bool triggered = false;

    void Start()
    {
        player1 = GameObject.Find("Ranged1Player");
        if (player1 == null)
        {
            player1 = GameObject.Find("RangedPlayer");
        }
        player2 = GameObject.Find("Melle1Player");
        if (player2 == null)
        {
            player2 = GameObject.Find("Melle2Player");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // فعال‌سازی دیالوگ
            dialoguePanel.SetActive(true);
            dialogueTreeScript.StartDialogue();  // راه‌اندازی دیالوگ
            player1.GetComponent<RangedPlayerController>().enabled = false; // غیرفعال‌کردن حرکت
            player2.GetComponent<MeleePlayerController>().enabled = false;
            player2.GetComponent<PlayerControllerBase>().enabled = false;
            player1.GetComponent<PlayerControllerBase>().enabled = false;
            
        }
    }
}