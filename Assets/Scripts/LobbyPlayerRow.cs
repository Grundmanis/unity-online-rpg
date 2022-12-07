using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyPlayerRow : MonoBehaviour
{
    [SerializeField]
    private GameObject playerName;

    [SerializeField]
    private GameObject kickButton;

    void Start()
    {
       playerName.GetComponent<TMP_Text>().text = "hello";
    }
}
