using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OnlineCharacterSelectionManager : NetworkBehaviour
{
    public static OnlineCharacterSelectionManager Instance;

    public CharacterSelectButton[] characterButtons;
    public Button ConfirmButton;

    private NetworkList<CharacterSelectState> players;

    private Color[] playerColors = new Color[2] { Color.red, Color.blue };
    private void Start()
    {
        if (IsServer && ConfirmButton != null)
        {
            ConfirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        players = new NetworkList<CharacterSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // اگر هاست در لیست نیست، به‌صورت دستی اضافه‌اش کن
            bool hostExists = false;
            foreach (var p in players)
            {
                if (p.clientID == NetworkManager.Singleton.LocalClientId)
                {
                    hostExists = true;
                    break;
                }
            }

            if (!hostExists)
            {
                Debug.Log("Host added manually to player list.");
                players.Add(new CharacterSelectState(NetworkManager.Singleton.LocalClientId, -1));
            }
        }

        if (IsClient)
        {
            players.OnListChanged += OnPlayerListChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (IsClient)
        {
            players.OnListChanged -= OnPlayerListChanged;
        }
    }

    private void Update()
    {
        if (ConfirmButton == null || players == null) return;

        foreach (var p in players)
        {
            if (p.clientID == NetworkManager.Singleton.LocalClientId)
            {
                ConfirmButton.interactable = p.characterID != -1;
            }
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        Debug.Log($"Player list changed: count = {players.Count}");
        UpdateUI();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log("Client connected: " + clientId);
        players.Add(new CharacterSelectState(clientId, -1));
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientID == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    public bool IsCharacterTaken(int characterID)
    {
        foreach (var player in players)
        {
            if (player.characterID == characterID)
                return true;
        }
        return false;
    }

    public void SelectCharacter(int characterID)
    {
        Debug.Log($"SelectCharacter called by client {NetworkManager.Singleton.LocalClientId} for character {characterID}");

        if (IsCharacterTaken(characterID))
        {
            Debug.LogWarning($"SelectCharacter: Character {characterID} is already taken");
            return;
        }

        SelectServerRpc(characterID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectServerRpc(int characterID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"SelectServerRpc called by client {serverRpcParams.Receive.SenderClientId} for character {characterID}");

        if (!IsServer)
        {
            Debug.LogWarning("SelectServerRpc: Called on client, ignoring");
            return;
        }

        ulong clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log(clientId + " Test1 ");
        Debug.Log(players.Count + " Test2 ");

        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log(players[i].clientID + " Test 3 ");
            if (players[i].clientID == clientId)
            {
                var state = players[i];

                bool taken = false;
                foreach (var p in players)
                {
                    if (p.characterID == characterID)
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken)
                {
                    state.characterID = characterID;
                    players[i] = state;
                    Debug.Log($"Character {characterID} assigned to client {clientId}");

                    if (IsServer && IsOwner)
                    {
                        UpdateUI();
                    }
                }
                else
                {
                    Debug.LogWarning($"SelectServerRpc: Character {characterID} already taken");
                }
                break;
            }
        }
    }

    public bool IsLocalPlayerReady()
    {
        foreach (var p in players)
        {
            if (p.clientID == NetworkManager.Singleton.LocalClientId)
            {
                return p.characterID != -1;
            }
        }
        return false;
    }

    private void UpdateUI()
    {
        for (int j = 0; j < characterButtons.Length; j++)
        {
            characterButtons[j].SetColor(Color.white);
        }

        Dictionary<ulong, Color> clientColors = new Dictionary<ulong, Color>();

        for (int i = 0; i < players.Count; i++)
        {
            ulong clientId = players[i].clientID;
            if (!clientColors.ContainsKey(clientId))
            {
                clientColors.Add(clientId, playerColors[clientColors.Count % playerColors.Length]);
            }
        }

        foreach (var player in players)
        {
            if (player.characterID != -1 && player.characterID < characterButtons.Length)
            {
                if (clientColors.TryGetValue(player.clientID, out Color colorToSet))
                {
                    characterButtons[player.characterID].SetColor(colorToSet);
                }
                else
                {
                    characterButtons[player.characterID].SetColor(Color.gray);
                }
            }
        }
    }
    public void OnConfirmButtonClicked()
    {
        if (!IsServer) return;

        int readyCount = 0;
        foreach (var p in players)
        {
            if (p.characterID != -1)
                readyCount++;
        }

        if (readyCount == 2)
        {
            Debug.Log("All players are ready. Starting game...");
            PlayerStateCache.Instance.Save(GetPlayerStates());            
            LoadGameScene();
        }
        else
        {
            Debug.LogWarning("Not all players are ready.");
        }
    }
    private void LoadGameScene()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Level 3", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
    public List<CharacterSelectState> GetPlayerStates()
    {
        List<CharacterSelectState> result = new List<CharacterSelectState>();
        foreach (var p in players)
        {
            result.Add(p);
        }
        return result;
    }


}