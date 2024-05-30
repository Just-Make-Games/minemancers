using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using System;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Transform lobbyPanel;
    [SerializeField] private TMP_Text lobbyName;

    [SerializeField] private Button leaveLobbyButton;

    [SerializeField] private Transform lobbyPlayerListParent;
    [SerializeField] private Transform lobbyPlayerListPrefab;

    [SerializeField] private Transform lobbyBlockerPanel;
    [SerializeField] private TMP_Text lobbyBlockerInfoText;

    private void Start()
    {
        NetworkedGameManager.OnLobbyInfoUpdated += LobbyInfoUpdated;
        NetworkedGameManager.OnIsAwaitingCallback += ManagerIsAwaitingCallback;

        leaveLobbyButton.onClick.AddListener(LeaveLobbyButton);
    }

    private void LeaveLobbyButton()
    {
        NetworkedGameManager.Singleton.LeaveLobby();
    }
    private void StartGameButton()
    {
        NetworkedGameManager.Singleton.StartRelayGame();
    }
    private void LobbyInfoUpdated(Lobby lobby, bool isPlayerHost)
    {
        if (lobby == null)
        {
            lobbyPanel.gameObject.SetActive(false);
            return;
        }

        lobbyPanel.gameObject.SetActive(true);
        lobbyName.text = lobby.Name;

        foreach (Transform child in lobbyPlayerListParent)
            Destroy(child.gameObject);

        foreach (var player in lobby.Players)
        {
            Transform newPrefab = Instantiate(lobbyPlayerListPrefab, lobbyPlayerListParent);
            TMP_Text prefabText = newPrefab.GetComponentInChildren<TMP_Text>();
            prefabText.text = player.Data["PlayerName"].Value;

            //as a host local player button is to start game, the rest is to kick
            if (!isPlayerHost)
                continue;

            Button playerPrefabButton = newPrefab.GetComponent<Button>();
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                playerPrefabButton.onClick.AddListener(NetworkedGameManager.Singleton.StartRelayGame);
                prefabText.text += "(click here to start game)";
            }
            else
            {
                playerPrefabButton.onClick.AddListener(() => NetworkedGameManager.Singleton.KickPlayerInLobby(player.Id));
                prefabText.text += "(click here to kick player)";
            }
        }
    }

    private void ManagerIsAwaitingCallback(bool isAwaitingCallback, string taskName)
    {
        lobbyBlockerPanel.gameObject.SetActive(isAwaitingCallback);
        lobbyBlockerInfoText.text = taskName;
    }
}
