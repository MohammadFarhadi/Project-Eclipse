using TMPro;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text emailDisplayText;

    void Start()
    {
        string userEmail = PlayerPrefs.GetString("user_email", "Unknown User");
        emailDisplayText.text = "Welcome, " + userEmail;
    }
}