using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using System;

public class LobbyListUI : MonoBehaviour
{
    
    [SerializeField] private Button refreshButton;

    [SerializeField] private Transform tableObject;

    [SerializeField] private Transform lobbyRowObject;

    [SerializeField] private Transform rowHeader;

    void Awake()
    {
        refreshButton.onClick.AddListener(() => MainLobby.Instance.RefreshLobbyList());
    }

    private void Start() {
        MainLobby.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }

       private void LobbyManager_OnLobbyListChanged(object sender, MainLobby.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {

        foreach (Transform child in tableObject.transform) {
            if (child == rowHeader || child == lobbyRowObject) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            Transform lobbySingleTransform = Instantiate(lobbyRowObject, tableObject);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyRowUI lobbyRowUI = lobbySingleTransform.GetComponent<LobbyRowUI>();
            lobbyRowUI.UpdateLobby(lobby);
        }
    }
}
