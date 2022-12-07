using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject tableObject;

    [SerializeField] private GameObject playerRowObject;

    [SerializeField] private TextMeshProUGUI lobbyNameText;

    [SerializeField] private TextMeshProUGUI isPrivatePublicText;

    [SerializeField] private TextMeshProUGUI playersText;

    void Start()
    {
        
        MainLobby.Instance.OnJoinedLobby += UpdateLobby_Event;

        //  GameObject.Instantiate(playerRowObject, Vector3.zero, Quaternion.identity, tableObject.transform);
        //  GameObject.Instantiate(playerRowObject, Vector3.zero, Quaternion.identity, tableObject.transform);
    }

    
    private void UpdateLobby_Event(object sender, MainLobby.LobbyEventArgs e) {
        UpdateLobby();
    }

    private void UpdateLobby() {
        UpdateLobby(MainLobby.Instance.GetJoinedLobby());
    }

     private void UpdateLobby(Lobby lobby) {


        // ClearLobby();

        // foreach (Player player in lobby.Players) {
        //     Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
        //     playerSingleTransform.gameObject.SetActive(true);
        //     LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

        //     lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
        //         LobbyManager.Instance.IsLobbyHost() &&
        //         player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
        //     );

        //     lobbyPlayerSingleUI.UpdatePlayer(player);
        // }

        // changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());

        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        isPrivatePublicText.text = lobby.IsPrivate ? "Private" : "Public";

        // Show();
    }
}
