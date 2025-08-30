using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject dialoguePanel;           // کل پنل دیالوگ
    public DialogueTree dialogueTreeScript;    // اسکریپت درخت دیالوگ
   
    

    private bool triggered = false;

   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Online)
            {
                if (other.TryGetComponent<NetworkObject>(out var netObj))
                {
                    if (!netObj.IsOwner)
                    {
                        // اگر owner نیست، کاری انجام نده
                        return;
                    }
                }
                triggered = true;

                // فعال‌سازی دیالوگ
                dialoguePanel.SetActive(true);
                dialogueTreeScript.StartDialogue();  // راه‌اندازی دیالوگ
               
            }
            else
            {
                triggered = true;

                // فعال‌سازی دیالوگ
                dialoguePanel.SetActive(true);
                dialogueTreeScript.StartDialogue();  // راه‌اندازی دیالوگ
                
            }
        }
    }
}