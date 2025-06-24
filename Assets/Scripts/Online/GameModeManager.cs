using UnityEngine;

public enum GameMode { Local, Online }

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode CurrentMode = GameMode.Local;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }
}