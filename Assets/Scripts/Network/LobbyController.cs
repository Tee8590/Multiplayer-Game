using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class LobbyController : MonoBehaviour
{

    public static LobbyController Instance { get; private set; }

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float timer;
    private float lobbyUpdateTimer;
    private string playerName;
    private string joinCode;
    private bool isClient;

    [SerializeField] private Transform playerListContainer; // Container for player names
    [SerializeField] private TextMeshProUGUI playerNamePrefab; // Prefab for player names
    [SerializeField] private TextMeshProUGUI joinCodeText;

    private List<string> players = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("PlayerID" + AuthenticationService.Instance.PlayerId);
            };
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("AuthenticationService.Instance.IsSignedIn" + AuthenticationService.Instance.IsSignedIn);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            playerName = "Player " + UnityEngine.Random.Range(10, 99);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    } 

    private void Update()
    {
        HandleLobbyTimer();
        HandleLobbyPollForUpdate();
    }

    private async void HandleLobbyTimer()
    {
        if (hostLobby != null) { 
            timer -= Time.deltaTime;

            if(timer < 0f)
            {
                float maxTime = 15;
                timer = maxTime;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer < 0f)
            {
                float maxTime = 1.1f;
                lobbyUpdateTimer = maxTime;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                Debug.Log("joinedLobby" + joinedLobby);
                if (joinedLobby.Data["JoinCode"].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        Debug.Log("IsLobbyHost" + IsLobbyHost());
                        bool isJoined = await RelayController.Instance.JoinRelay(joinedLobby.Data["JoinCode"].Value);

                        if (isJoined) { GoToGameScene(true); }
                    }
                }
            }
        }
    }

    public async Task<bool> CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayerCount = 4;

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                    {
                        {
                            "GameMode" , new(DataObject.VisibilityOptions.Public, "Multi Player")
                        },
                        {
                            "JoinCode" , new(DataObject.VisibilityOptions.Member, "0")
                        }
                    }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayerCount, createLobbyOptions);
            Debug.Log($"Lobby Created: Name = {lobby.Name}, ID = {lobby.Id}, MaxPlayers = {lobby.MaxPlayers}");
            hostLobby = joinedLobby = lobby;
            ListLobbies();
            PrintPlayers(hostLobby);
            return true;
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
            return false;
        }
    }

    private async void ListLobbies()
    {
        try
        {
            Debug.Log("List");
            QueryLobbiesOptions query = new()
            {
                Count = 20,
                Filters = new List<QueryFilter>{
                    //new (QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new (false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(query);
            Debug.Log($"Number of Lobbies Found: {queryResponse.Results.Count}");
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("name : " + lobby.Name + " max players : " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }

    }

    public async Task<bool> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = GetPlayer()
            };

            lobbyCode = lobbyCode.Trim();
            ListLobbies();
            Debug.Log($"Attempting to join lobby with code: {lobbyCode}");

            if (string.IsNullOrEmpty(lobbyCode))
            {
                Debug.LogError("Lobby code is null or empty");
                return false;
            }

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;

            PrintPlayers(joinedLobby);
            return false;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
            return false;
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            string playerName = player.Data.ContainsKey("PlayerName")
            ? player.Data["PlayerName"].Value
            : "Unknown Player";
            AddPlayerToList(playerName);
        }
    }

    private void AddPlayerToList(string playerName)
    {
        players.Add(playerName);

        TextMeshProUGUI playerNameText = Instantiate(playerNamePrefab, playerListContainer);
        playerNameText.text = playerName;
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            "PlayerName" , new(PlayerDataObject.VisibilityOptions.Member, playerName)
                        }
                    }
        };
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { "GameMode", new(DataObject.VisibilityOptions.Public, gameMode) } }
            });

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e) 
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "Player Name", new(PlayerDataObject.VisibilityOptions.Member, playerName)
                    }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private async void UpdateHost(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = AuthenticationService.Instance.PlayerId
            });

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            GoToLobbyScene();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    public async Task<bool> StartGame()
    {
        if(IsLobbyHost())
        {
            try
            {
                string relayCode = await RelayController.Instance.CreateRelay();
                Debug.Log("relayCode : " + relayCode);
                joinCodeText.text = relayCode;
                Debug.Log("Before Lobby Data: " + joinedLobby.Data["JoinCode"].Value);
                Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> { { "JoinCode", new(DataObject.VisibilityOptions.Member, relayCode) } }
                });

                joinedLobby = lobby;
                Debug.Log("Updated Lobby Data: " + joinedLobby.Data["JoinCode"].Value);
                return true;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("LobbyServiceException : " + e);
                return false;
            }
        }
        return false;
    }

    public void GoToGameScene(bool client = false)
    {
        isClient = client;
        if(joinedLobby != null)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("MultiPlayer");
        }
    }

    private void GoToLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }

    private bool IsLobbyHost()
    {
        if(joinedLobby.HostId == AuthenticationService.Instance.PlayerId) {  return true; }
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MultiPlayer")
        {
            if (RelayController.Instance != null)
            {
                if (!isClient)
                {
                    Debug.Log("Host");
                    NetworkManager.Singleton.StartHost();
                }
                else
                {
                    Debug.Log("Client");
                    NetworkManager.Singleton.StartClient();
                }
                
            }
            else
            {
                Debug.LogError("RelayController instance not found in MultiPlayer scene.");
            }
        }
    }
}
