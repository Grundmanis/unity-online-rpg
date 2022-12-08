using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine.UI;

public class LobbyPlayerRow : MonoBehaviour
{
    [SerializeField]
    private GameObject playerName;

    [SerializeField]
    private Button kickButton;

    private Player player;

    private void Awake() {
        kickButton.onClick.AddListener(KickPlayer);
    }

    public void UpdatePlayer(Player player) {

        this.player = player;
        Debug.Log("Update player");
        playerName.GetComponent<TMP_Text>().text = player.Data["PlayerName"].Value;

        // Don't allow kick self
        if (MainLobby.Instance.IsLobbyHost() && player.Id != AuthenticationService.Instance.PlayerId) {
            kickButton.gameObject.SetActive(true);
        }
    }

        private void KickPlayer() {
        if (player != null) {
            MainLobby.Instance.KickPlayer(player.Id);
        }
    }
}
