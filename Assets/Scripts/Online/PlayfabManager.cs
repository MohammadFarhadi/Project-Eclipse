using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayfabManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_Text errorText;

    public void Signup()
    {
        var request = new RegisterPlayFabUserRequest {
            Email = email.text,
            Password = password.text,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnSignupSuccess, OnError);
    }

    public void Login()
    {
        var request = new LoginWithEmailAddressRequest {
            Email = email.text,
            Password = password.text,
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    public void RecoverPassword()
    {
        var request = new SendAccountRecoveryEmailRequest {
            Email = email.text,
            TitleId = "109447"
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnRecoverySuccess, OnError);
    }

    void OnSignupSuccess(RegisterPlayFabUserResult result){
        PlayerPrefs.SetString("user_email", email.text);
        PlayerPrefs.Save();
        Debug.Log("Signup Successful");
        SceneManager.LoadScene("Lobby");
    }

    void OnLoginSuccess(LoginResult result)
    {
        PlayerPrefs.SetString("user_email", email.text);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Lobby");
    }

    void OnRecoverySuccess(SendAccountRecoveryEmailResult result){
        Debug.Log("Email Sent!");
    }

    void OnError(PlayFabError error){
        errorText.text = error.GenerateErrorReport();
    }
}
