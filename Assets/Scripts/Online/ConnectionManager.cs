// Scripts/ConnectionManager.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using TMPro;


public class ConnectionManager : MonoBehaviour
{
    public TMP_InputField ipInputField; // تنظیمش کن از Inspector
    public string gameSceneName = "GameScene";

    public void OnHostClicked()
    {
        NetworkInfo.DetectHostIP(); // آی‌پی هاست گرفته میشه
        Debug.Log("Host IP: " + NetworkInfo.HostIP);

        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnClientClicked()
    {
        var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        transport.ConnectionData.Address = ipInputField.text;

        NetworkManager.Singleton.StartClient();
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnClientSceneLoaded;
    }

    private void OnClientSceneLoaded(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == gameSceneName)
        {
            // مطمئن شدیم وارد همون سین شد
            Debug.Log("Client joined game scene successfully.");
            // فقط یک بار نیاز داریم
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnClientSceneLoaded;
        }
    }
}