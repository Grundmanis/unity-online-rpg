using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using IngameDebugConsole;
using TMPro;
using System;

public class MainLobby : MonoBehaviour
{
    public static MainLobby Instance { get; private set; }

    private Lobby hostLobby; 
    
    private Lobby joinedLobby; 

    private float hearbeatTimer;

    private float lobbyUpdateTimer;

    private string playerName;

    // private string lobbyName;

    // private bool IsPrivateValue = false;

    // private int maxPlayers = 5;

    [SerializeField]
    private GameObject playerInputField;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;

    public class LobbyEventArgs : EventArgs {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;

    public class OnLobbyListChangedEventArgs : EventArgs {
        public List<Lobby> lobbyList;
    }

    private async void Awake()
    {
        Instance = this;

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        // Later implement Steam signin
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "GameFeel " + UnityEngine.Random.Range(10, 99);
        Debug.Log("Player name: " + playerName);

        playerInputField.GetComponent<TMP_InputField>().text = playerName;
        
		DebugLogConsole.AddCommand<string>( "JoinLobby", "join lobby by code ", JoinLobbyByCode);
    }

    public bool IsLobbyHost() {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private void Update() {
        // HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    // Inactive
    private async void HandleLobbyHeartbeat() {
        if (hostLobby != null) {
            hearbeatTimer -= Time.deltaTime;

            if (hearbeatTimer < 0f) {
                float hearbeatTimerMax = 15;
                hearbeatTimer = hearbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates() {
        if (joinedLobby != null) {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer < 0f) {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate) {

        try {

            Player player = GetPlayer();

            CreateLobbyOptions options = new CreateLobbyOptions() {
                Player = player,
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject> {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Raid")}
                    // { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "PrivateRaid", DataObject.IndexOptions.S1)}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;

            joinedLobby = hostLobby;

            Debug.Log("PlayerId" + AuthenticationService.Instance.PlayerId);
            Debug.Log("Lobby created! Name: " + lobbyName + "; Lobby id: " + lobby.Id + "; Lobby code: " + lobby.LobbyCode + " Max Players: " + maxPlayers + "; is private: " + options.IsPrivate);
            
            OnJoinedLobby?.Invoke(Instance, new LobbyEventArgs { lobby = lobby });
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void RefreshLobbyList() {
        try {

            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                Count = 25,
                Filters = new List<QueryFilter> {
                    // new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    // new QueryFilter(QueryFilter.FieldOptions.S1, "PrivateRaid", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = response.Results });

            Debug.Log("Lobbies found: " + response.Results.Count);

            foreach(Lobby lobby in response.Results) {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + "; Game mode: " + lobby.Data["GameMode"].Value);
            }
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void JoinLobbyByCode(string lobbyCode) {
        
        try {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Joined to lobby with code " + lobbyCode);
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(Lobby lobby) {
        Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions {
            Player = player
        });

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    private Player GetPlayer() {
        return new Player() {
                    Data = new Dictionary<string, PlayerDataObject> {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
                };
    }

    // private async void updateLobbyGameMode(string gameMode) {
    //     try {
    //         hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
    //             Data = new Dictionary<string, DataObject> {
    //                 { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
    //             }
    //         });
    //         joinedLobby = hostLobby;

    //    } catch(LobbyServiceException e) {
    //         Debug.Log(e);
    //     }
    // }

    public async void LeaveLobby() {

        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
       } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void KickPlayer(string playerId) {
       if (IsLobbyHost()) {
            try {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            } catch (LobbyServiceException e) {
                Debug.Log(e);
            }
        }
    }

    public async void setPlayerName(string newPlayerName) {
        Debug.Log("new player name: " + newPlayerName);
        playerName = newPlayerName;

        if (joinedLobby != null) {
               await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
            
            OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
    }
    
    public Lobby GetJoinedLobby() {
        return joinedLobby;
    }
}
