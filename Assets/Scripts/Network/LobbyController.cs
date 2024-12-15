using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController
{

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float timer;
    private float lobbyUpdateTimer;
    private string playerName;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("PlayerID" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Player " + Random.Range(10, 99);
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

                if (joinedLobby.Data["JoinCode"].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        RelayController.Instance.JoinRelay(joinedLobby.Data["JoinCode"].Value);
                    }
                }
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayerCount = 4;

            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = true,
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

            hostLobby = joinedLobby = lobby;

            PrintPlayers(hostLobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions query = new()
            {
                Count = 20,
                Filters = new List<QueryFilter>{
                    new (QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new (false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(query);

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

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("LobbyServiceException : " + e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (var player in lobby.Players)
        {
            Debug.Log($"{player}");
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            "Player Name" , new(PlayerDataObject.VisibilityOptions.Member, playerName)
                        }
                    }
        };
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
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
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
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

    private async void StartGame()
    {
        if(IsLobbyHost())
        {
            try
            {
                string relayCode = await RelayController.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> { { "JoinCode", new(DataObject.VisibilityOptions.Public, relayCode) } }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("LobbyServiceException : " + e);
            }
        }
    }

    private void GoToGameScene()
    {
        if(joinedLobby != null)
            SceneManager.LoadScene("MultiPlayer");
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
}
