using System.Collections.Generic;
using UnityEngine;

public class PlayerStateCache : MonoBehaviour
{
    public static PlayerStateCache Instance;

    public List<CharacterSelectState> playerStates;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save(List<CharacterSelectState> states)
    {
        playerStates = states;
    }
}