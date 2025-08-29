using Unity.Netcode;
            using UnityEngine;
            
            public class PlayerSpawnerManager : NetworkBehaviour
            {
                public static PlayerSpawnerManager Instance;
            
                public GameObject[] characterPrefabs;
                public Transform[] spawnPoints;
            
                private static bool s_alreadySpawned;   // برای این صحنه

                private void OnDisable() { s_alreadySpawned = false; } // با خروج/تعویض صحنه ریست شود
                public void Awake()
                {
                    Debug.Log("PlayerSpawnerManager Awake called");
                    if (Instance != null && Instance != this)
                    {
                        Debug.Log("Destroying duplicate PlayerSpawnerManager");
                        Destroy(gameObject);
                    }
                    else
                    {
                        Instance = this;
                        Debug.Log("PlayerSpawnerManager Instance set");
                    }
                }
            
                public override void OnNetworkSpawn()
                {
                    Debug.Log("OnNetworkSpawn called. CurrentMode: " + GameModeManager.Instance.CurrentMode + ", IsServer: " + IsServer);
                    if (GameModeManager.Instance.CurrentMode == GameMode.Online && IsServer)
                    {
                        Debug.Log("Spawning players for online mode...");
                        SpawnAllOnlinePlayers();
                    }
                }
            
                public void Start()
                {
                    Debug.Log("PlayerSpawnerManager Start called. CurrentMode: " + GameModeManager.Instance.CurrentMode);
                    if (GameModeManager.Instance.CurrentMode == GameMode.Local)
                    {
                        if (!SaveSystem.IsRestoring)
                        {
                            Debug.Log("Spawning local players...");
                            SpawnLocalPlayers();
                        }
                        else
                        {
                            Debug.Log("SaveSystem is restoring, skipping auto-spawn.");
                        }
                    }
                }
                
                
            
                public void SpawnLocalPlayers()
                {
                    var existingPlayers = FindObjectsOfType<PlayerControllerBase>(true);
                    if (existingPlayers.Length >= 2)
                    {
                        Debug.LogWarning("[Spawner] Players already exist (" + existingPlayers.Length + "), skipping spawn.");
                        return;
                    }

                    // اگر قبلاً همین صحنه اسپاون کرده‌ایم، نزن دوباره
                    if (s_alreadySpawned)
                    {
                        Debug.LogWarning("[Spawner] SpawnLocalPlayers called twice. Skipping.");
                        return;
                    }
                    s_alreadySpawned = true;
                    Debug.Log("SpawnLocalPlayers called");
                    int[] selectedChars = null;
                    if (SaveSystem.RestoreMeleeID != -1 && SaveSystem.RestoreRangedID != -1)
                    {
                        // your spawner expects index 0 -> player at spawnPoints[0], etc.
                        // You previously used [ranged, melee] order; keep whatever your game expects:
                        selectedChars = new int[] { SaveSystem.RestoreRangedID, SaveSystem.RestoreMeleeID };
                        Debug.Log($"[Spawner] Using restore IDs Ranged={SaveSystem.RestoreRangedID}, Melee={SaveSystem.RestoreMeleeID}");
                    }
                    else
                    {
                        selectedChars = LocalCharacterSelectionManager.Instance.GetSelectedCharacters();

                    }
                    Debug.Log("Selected characters: " + string.Join(", ", selectedChars));
                    GameObject selectedRangedPlayer = null;
                    GameObject selectedMeleePlayer = null;
            
                    for (int i = 0; i < selectedChars.Length; i++)
                    {
                        int characterID = selectedChars[i];
                        Debug.Log($"Spawning player {i + 1} with characterID: {characterID}");
                        if (characterID >= 0 && characterID < characterPrefabs.Length)
                        {
                            GameObject obj = Instantiate(characterPrefabs[characterID], spawnPoints[i].position, Quaternion.identity);
                            var baseCtrl = obj.GetComponent<PlayerControllerBase>();
                            baseCtrl.CharacterID.Value = characterID;
            
                            PlayersUI p;
                            if (characterID < 2)
                            {
                                p = GameObject.Find("MeleeUIManager  ").GetComponent<PlayersUI>();
                                selectedMeleePlayer = obj;
                                Debug.Log("Assigned MeleeUIManager to player");
                            }
                            else
                            {
                                p = GameObject.Find("RangedUIManager  ").GetComponent<PlayersUI>();
                                selectedRangedPlayer = obj;
                                Debug.Log("Assigned RangedUIManager to player");
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
            
                    if (selectedMeleePlayer != null && selectedRangedPlayer != null)
                    {
                        Debug.Log("Setting up cameras for local players");
                        CameraManager camManager = FindObjectOfType<CameraManager>();
                        camManager.player1 = selectedRangedPlayer;
                        camManager.player2 = selectedMeleePlayer;
            
                        camManager.camera1 = selectedRangedPlayer.GetComponentInChildren<Camera>();
                        camManager.camera2 = selectedMeleePlayer.GetComponentInChildren<Camera>();
                    }
                }
            
                public void SpawnAllOnlinePlayers()
                {
                    Debug.Log("SpawnAllOnlinePlayers called");
                    if (!IsServer)
                    {
                        Debug.Log("Not server, aborting online spawn");
                        return;
                    }
            
                    var players = PlayerStateCache.Instance.playerStates;
                    Debug.Log("PlayerStateCache count: " + players.Count);
            
                    int spawnIndex = 0;
                    foreach (var player in players)
                    {
                        Debug.Log($"Spawning online player {player.clientID} with characterID: {player.characterID}");
                        if (player.characterID >= 0 && player.characterID < characterPrefabs.Length)
                        {
                            GameObject playerObj = Instantiate(characterPrefabs[player.characterID], spawnPoints[spawnIndex].position, Quaternion.identity);
                            playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(player.clientID);
                            var baseCtrl = playerObj.GetComponent<PlayerControllerBase>();
                            baseCtrl.CharacterID.Value = player.characterID;
            
                            ClientRpcParams rpcParams = new ClientRpcParams
                            {
                                Send = new ClientRpcSendParams
                                {
                                    TargetClientIds = new ulong[] { player.clientID }
                                }
                            };
                            baseCtrl.SetupCameraClientRpc(rpcParams);
            
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