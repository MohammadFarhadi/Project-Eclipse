using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadinLvl2 : MonoBehaviour
{
    public Animator doorAnimator; // Animator در
    private bool player1Inside = false;
    private bool player2Inside = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "RangedPlayer")
            player1Inside = true;
        else if (other.gameObject.name == "Ranged1Player")
        {
            player1Inside = true;
        }
        if (other.gameObject.name == "Melle1player")
            player2Inside = true;
        else if (other.gameObject.name == "Melle2Player")
        {
            player2Inside = true;
        }

        CheckIfBothPlayersInside();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "RangedPlayer")
            player1Inside = false;
        else if (other.gameObject.name == "Ranged1Player")
        {
            player1Inside = false;
        }
        if (other.gameObject.name == "Melle1player")
            player2Inside = false;
        else if (other.gameObject.name == "Melle2Player")
        {
            player2Inside = false ;
        }
    }

    void CheckIfBothPlayersInside()
    {
        if (doorAnimator != null && doorAnimator.GetBool("isOpen"))
        {
            if (player1Inside && player2Inside)
            {
                Debug.Log("Both players inside and door is open!");
                SceneManager.LoadScene("Level2"); // اسم دقیق Scene بعدی
            }
        }
    }
}