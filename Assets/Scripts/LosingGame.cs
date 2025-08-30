using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LosingGame : NetworkBehaviour
{
    public Boolean _isPlayer1Dead = false;
    public Boolean _isPlayer2Dead = false;
    // Update is called once per frame
    void Update()
    {
        if (_isPlayer1Dead && _isPlayer2Dead)
        {
            if (GameModeManager.Instance.CurrentMode == GameMode.Local)
            {
                SceneManager.LoadScene("Game Over");
            }
            else
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("Game Over", UnityEngine.SceneManagement.LoadSceneMode.Single);

                }
            }
        }
    }
}
