using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using IngameDebugConsole;
using TMPro;

public class MainLobby : MonoBehaviour
{
    private Lobby hostLobby; 
    
    private Lobby joinedLobby; 

    private float hearbeatTimer;

    private float lobbyUpdateTimer;

    private string playerName;

    private string lobbyName;

    private bool IsPrivateValue = false;

    private int maxPlayers = 5;

    [SerializeField]
    private GameObject playerInputField;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        // Later implement Steam signin
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "GameFeel " + UnityEngine.Random.Range(10, 99);
        Debug.Log("Player name: " + playerName);

        playerInputField.GetComponent<TMP_InputField>().text = playerName;
        
		DebugLogConsole.AddCommand( "CreateLobby", "Creates lobby ", CreateLobby);
		DebugLogConsole.AddCommand( "ListLobbies", "List lobby ", ListLobbies);
		DebugLogConsole.AddCommand( "QuickJoinLobby", "Quick Join Lobby ", QuickJoinLobby);
		DebugLogConsole.AddCommand( "PrintPlayers", "PrintPlayers ", PrintPlayers);
		DebugLogConsole.AddCommand<string>( "JoinLobby", "join lobby by code ", JoinLobbyByCode);
		DebugLogConsole.AddCommand<string>( "UpdateLobbyGameMode", "update lobby game mode", updateLobbyGameMode);
		DebugLogConsole.AddCommand<string>( "UpdatePlayerName", "UpdatePlayerName", UpdatePlayerName);
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
            }
        }
    }

    public async void CreateLobby() {
        try {
            CreateLobbyOptions options = new CreateLobbyOptions() {
                IsPrivate = IsPrivateValue,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "PrivateRaid")}
                    // { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "PrivateRaid", DataObject.IndexOptions.S1)}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            Debug.Log("Lobby created! Name: " + lobbyName + "; Lobby id: " + lobby.Id + "; Lobby code: " + lobby.LobbyCode + " Max Players: " + maxPlayers + "; is private: " + options.IsPrivate);
            
            PrintPlayers(hostLobby);
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void ListLobbies() {
        try {

            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    // new QueryFilter(QueryFilter.FieldOptions.S1, "PrivateRaid", QueryFilter.OpOptions.EQ)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

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

            PrintPlayers(joinedLobby);

        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void QuickJoinLobby() {
         try {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            joinedLobby = lobby;
            Debug.Log("Quick joined to lobby with code " + lobby.LobbyCode);
        } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private Player GetPlayer() {
        return new Player() {
                    Data = new Dictionary<string, PlayerDataObject> {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                    }
                };
    }

    private void PrintPlayers() {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby) {
        Debug.Log("Players in lobby: " + lobby.Name + "; Game mode: " + lobby.Data["GameMode"].Value);
        foreach(Player player in lobby.Players) {
            Debug.Log("Player id: " + player.Id + " ; data: " + player.Data["PlayerName"].Value);
        }
    }

    private async void updateLobbyGameMode(string gameMode) {
        try {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });
            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);

       } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName) {

        try {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
       } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
        
    }

    private async void LeaveLobby() {

        try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
       } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    private async void KickPlayer() {
          try {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
       } catch(LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public void setPlayerName(string newPlayerName) {
        Debug.Log("new player name: " + newPlayerName);
        playerName = newPlayerName;
    }

    public void setLobbbyName(string newLobbyName) {
        Debug.Log("new lobby name: " + newLobbyName);
        lobbyName = newLobbyName;
    }

    public void setMaxPlayers(string newMaxPlayers) {
        
         if (int.TryParse(newMaxPlayers, out int result)) {
            Debug.Log("new max players: " + result);
            maxPlayers = result;
        } else {
            Debug.Log("Could not set max players, setting to 5");
            maxPlayers = 5;
        }
    }

    public void toggleIsPrivate() {
        IsPrivateValue = !IsPrivateValue;
        Debug.Log("is private now " + IsPrivateValue);
    }
}
