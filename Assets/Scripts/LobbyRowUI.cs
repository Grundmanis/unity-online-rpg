using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using TMPro;
using UnityEngine.UI;

public class LobbyRowUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyName;
    
    [SerializeField] private GameObject privatePublicName;

    [SerializeField] private GameObject playerInLobby;
    
    [SerializeField] private Button joinButton;

    private Lobby lobby;

    void Awake()
    {
        joinButton.onClick.AddListener(() => MainLobby.Instance.JoinLobby(lobby));
    }

    public void UpdateLobby(Lobby lobby) {

        this.lobby = lobby;
        
        lobbyName.GetComponent<TMP_Text>().text = lobby.Name;
        privatePublicName.GetComponent<TMP_Text>().text = lobby.IsPrivate ? "Private" : "Public";
        playerInLobby.GetComponent<TMP_Text>().text = lobby.Players.Count + "/" + lobby.MaxPlayers;

        if (lobby.Players.Count == lobby.MaxPlayers) {
            joinButton.gameObject.SetActive(false);
        }
    }
}
