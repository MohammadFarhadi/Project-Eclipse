using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnerManager : NetworkBehaviour
{
    public static PlayerSpawnerManager Instance;

    public GameObject[] characterPrefabs; // باید از قبل در Inspector پر بشه
    public Transform[] spawnPoints;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Online && IsServer)
        {
            Debug.Log("Spawning players for online mode...");
            SpawnAllOnlinePlayers();
        }
    }

    private void Start()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Local)
        {
            Debug.Log("Spawning players for local mode...");
            SpawnLocalPlayers();
        }
    }

    // لوکال دو بازیکن را طبق انتخابشان اسپان می‌کند
    private void SpawnLocalPlayers()
    {
        int[] selectedChars = LocalCharacterSelectionManager.Instance.GetSelectedCharacters();

        for (int i = 0; i < selectedChars.Length; i++)
        {
            int characterID = selectedChars[i];
            if (characterID >= 0 && characterID < characterPrefabs.Length)
            {
                GameObject obj = Instantiate(characterPrefabs[characterID], spawnPoints[i].position, Quaternion.identity);
                PlayersUI p; 
                if (characterID < 2)
                {
                    p = GameObject.Find("MeleeUIManager  ").GetComponent<PlayersUI>();
                }
                else
                {
                    p = GameObject.Find("RangedUIManager  ").GetComponent<PlayersUI>();  
                }
                obj.GetComponent<PlayerControllerBase>().SetPlayerUI(p);
                obj.GetComponent<PlayerControllerBase>().RefreshUI();
                Debug.Log($"Local player {i + 1} spawned with characterID: {characterID}");
            }
            else
            {
                Debug.LogError($"Invalid characterID {characterID} for player {i + 1}");
            }
        }
    }

    // آنلاین با توجه به انتخاب بازیکنان، آنها را با NetworkObject اسپان می‌کند
    private void SpawnAllOnlinePlayers()
    {
        if (!IsServer) return;

        var players = PlayerStateCache.Instance.playerStates;

        int spawnIndex = 0;
        foreach (var player in players)
        {
            if (player.characterID >= 0 && player.characterID < characterPrefabs.Length)
            {
                GameObject playerObj = Instantiate(characterPrefabs[player.characterID], spawnPoints[spawnIndex].position, Quaternion.identity);
                playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(player.clientID);
                bool isMelee = player.characterID < 2;
                playerObj.GetComponent<PlayerControllerBase>().SetPlayerUIClientRpc(isMelee);
                Debug.Log($"Online player {player.clientID} spawned with characterID: {player.characterID}");
                spawnIndex++;
            }
            else
            {
                Debug.LogWarning($"Skipping player {player.clientID} due to invalid characterID: {player.characterID}");
            }
        }
    }
}