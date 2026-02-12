using System;
using UnityEngine;

public class LogTerminalTrigger : MonoBehaviour
{
    private bool playerInRange = false;
     PlayerControllerBase currentPlayer;
     

    void Update()
    {
        if (playerInRange && currentPlayer.IsInteracting())
        {
            TerminalUIManager.Instance.StartTerminal();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
        if (player != null)
        {
            currentPlayer = player;
        }
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}