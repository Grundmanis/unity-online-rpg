using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Transform tableObject;

    [SerializeField] private Transform playerRowObject;

    [SerializeField] private Transform rowHeader;

    [SerializeField] private TextMeshProUGUI lobbyNameText;

    [SerializeField] private TextMeshProUGUI isPrivatePublicText;

    [SerializeField] private TextMeshProUGUI playersText;

    [SerializeField] private Button leaveButton;

    void Awake() {
        leaveButton.onClick.AddListener(() => MainLobby.Instance.LeaveLobby());
    }

    void Start()
    {
        MainLobby.Instance.OnJoinedLobby += UpdateLobby_Event;
        MainLobby.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;

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

        foreach (Transform child in tableObject) {
            if (child == rowHeader || child == playerRowObject) continue;

            Destroy(child.gameObject);
        }

        foreach (Player player in lobby.Players) {
            
            Transform playerSingleTransform = Instantiate(playerRowObject, tableObject);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerRow lobbyPlayerRow = playerSingleTransform.GetComponent<LobbyPlayerRow>();
            lobbyPlayerRow.UpdatePlayer(player);
        }

        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        isPrivatePublicText.text = lobby.IsPrivate ? "Private" : "Public";
    }
}
