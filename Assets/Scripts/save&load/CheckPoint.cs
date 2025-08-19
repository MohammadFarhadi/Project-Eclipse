using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private float cooldownTime = 2f; // Cooldown time in seconds
    private float cooldownTimer = 5f;
    
    RangedPlayerController GetActiveRanged()
    {
        foreach (var ranged in FindObjectsOfType<RangedPlayerController>())
        {
            if (ranged.gameObject.activeInHierarchy)
                return ranged;
        }
        return null;
    }

    MeleePlayerController GetActiveMelee()
    {
        foreach (var melee in FindObjectsOfType<MeleePlayerController>())
        {
            if (melee.gameObject.activeInHierarchy)
                return melee;
        }
        return null;
    }

    
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name + " has entered the checkpoint trigger.");
        Debug.Log(cooldownTimer);
        if (other.CompareTag("Player"))
        {
            
            // Save the player's position when they hit the checkpoint
            PlayerControllerBase player = other.GetComponent<PlayerControllerBase>();
            if (player != null)
            {
                
                if (cooldownTimer <= cooldownTime)
                {             
                    
                    cooldownTimer += Time.deltaTime * 10f;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        
                        // Reset the cooldown timer
                        cooldownTimer = 0f;
                        //save part here
                        SaveSystem.SaveData(GetActiveMelee(), GetActiveRanged());
                        Debug.Log("Checkpoint saved at: " + transform.position);
                    }

                    if (Input.GetKeyDown(KeyCode.Y))
                    {
                        SaveSystem.LoadData();
                    }
                    // cooldownTimer = 0f;
                    // //save part here
                    // SaveSystem.SaveData(GetActiveMelee(),GetActiveRanged());
                    // Debug.Log("Checkpoint saved at: " + transform.position);
                }
            }
            else
            {
                Debug.LogWarning("PlayerControllerBase component not found on the player.");
            }
        }
    }
}
