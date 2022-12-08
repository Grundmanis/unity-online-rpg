using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInput;

    [SerializeField] private TMP_InputField maxPlayersInput;

    [SerializeField] private TextMeshProUGUI publicPrivateText;

    [SerializeField] private Button isPrivateButton;

    [SerializeField] private Button createLobbyButton;


    private string lobbyName;

    private int maxPlayers = 5;

    private bool isPrivate = false;

    
    // Start is called before the first frame update
    void Start()
    {
        lobbyNameInput.onValueChanged.AddListener(UpdateLobbyName);
        maxPlayersInput.onValueChanged.AddListener(UpdateMaxPlayers);
        isPrivateButton.onClick.AddListener(ToggleIsPrivate);
        createLobbyButton.onClick.AddListener(() => {
             MainLobby.Instance.CreateLobby(
                lobbyName,
                maxPlayers,
                isPrivate
            );
        });
    }

    private void UpdateLobbyName(string newLobbyName) {

        lobbyName = newLobbyName;
    }

    private void UpdateMaxPlayers(string newMaxPlayers) {

          if (int.TryParse(newMaxPlayers, out int result)) {
            Debug.Log("new max players: " + result);
            maxPlayers = result;
        } else {
            Debug.Log("Could not set max players, setting to 5");
            maxPlayers = 5;
        }
    }

    private void ToggleIsPrivate() {

        isPrivate = !isPrivate;
        publicPrivateText.text = isPrivate ? "Private" : "Public";
        Debug.Log("is private now " + isPrivate);
    }
}
