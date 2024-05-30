using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class MenuUI : MonoBehaviour
{
    public static void Quit()
    {
        Application.Quit();
    }
    
    public static string PlayerName = "Player";

    [Header("Blocker panel")]
    [SerializeField] private GameObject blockUIPanel;
    [SerializeField] private TMP_Text blockUIText;

    [Header("Options")]
    [SerializeField] private TMP_InputField playerName;

    [Header("Start game")]
    [SerializeField] private Button createLobby;
    [SerializeField] private Button createLocal;

    [Header("Quick join and Direct Connect")]
    [SerializeField] private TMP_InputField quickJoinCodeInputField;
    [SerializeField] private Button joinByCodeButton;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button directConnectButton;

    [Header("Lobby List")]
    [SerializeField] private Button refreshLobbyListButton;
    [SerializeField] private Transform lobbyParent;
    [SerializeField] private Button lobbyPrefab;

    [Header("Lobby Info Panel")]
    [SerializeField] private Transform lobbyInfoPanel;
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private Button closeLobbyInfoButton;
    [SerializeField] private Button connectButton;
    [SerializeField] private Transform lobbyPlayerListParent;
    [SerializeField] private Transform lobbyPlayerListPrefab;
    
    private Lobby _selectedLobby;
    
    private void Start()
    {
        NetworkedGameManager.OnLobbyAttemptSuccess += LobbyAttemptSuccess;
        NetworkedGameManager.OnLobbyAttemptFailed += LobbyAttemptFailed;
        NetworkedGameManager.OnGetLobbiesSuccess += GetLobbiesSuccess;
        
        createLobby.onClick.AddListener(CreateLobby);
        createLocal.onClick.AddListener(CreateLocal);
        
        refreshLobbyListButton.onClick.AddListener(GetLobbiesButton);
        
        joinByCodeButton.onClick.AddListener(JoinLobbyByCodeButton);
        directConnectButton.onClick.AddListener(DirectConnect);
        
        closeLobbyInfoButton.onClick.AddListener(CloseLobbyInfoButton);
        connectButton.onClick.AddListener(ConnectButton);
        
        playerName.text = PlayerName;
        playerName.onValueChanged.AddListener(PlayerNameChanged);
    }
    
    private void PlayerNameChanged(string newPlayerName)
    {
        PlayerName = newPlayerName;
    }

    private void DisableUIWhileWaiting(string textToDisplay)
    {
        blockUIPanel.SetActive(true);
        blockUIText.text = textToDisplay;
    }
    private void ReenableUI()
    {
        blockUIPanel.SetActive(false);
    }


    private void CreateLobby()
    {
        DisableUIWhileWaiting("Creating Lobby");
        NetworkedGameManager.Singleton.CreateLobby();
    }
    private void CreateLocal()
    {
        NetworkedGameManager.Singleton.CreateLocal();
    }

    private void DirectConnect()
    {
        NetworkedGameManager.Singleton.ConnectToIP(ipInputField.text);
    }

    private void ConnectButton()
    {
        DisableUIWhileWaiting("Attempting to connect...");
        NetworkedGameManager.Singleton.JoinLobby(_selectedLobby);
    }

    private void JoinLobbyByCodeButton()
    {
        DisableUIWhileWaiting("Attempting to connect...");
        NetworkedGameManager.Singleton.JoinLobbyByCode(quickJoinCodeInputField.text);
    }

    private void GetLobbiesButton()
    {
        UpdateLobbyInfo(null);
        NetworkedGameManager.Singleton.GetLobbies();
    }

    private void CloseLobbyInfoButton()
    {
        UpdateLobbyInfo(null);
    }
    private void LobbyButtonAction(Lobby lobby)
    {
        UpdateLobbyInfo(lobby);
    }
    private void UpdateLobbyInfo(Lobby lobby)
    {
        _selectedLobby = lobby;

        if (_selectedLobby == null)
        {
            lobbyInfoPanel.gameObject.SetActive(false);
            return;
        }

        lobbyInfoPanel.gameObject.SetActive(true);

        lobbyNameText.text = lobby.Name;

        foreach (Transform child in lobbyPlayerListParent)
            Destroy(child.gameObject);

        foreach (var player in lobby.Players)
        {
            Transform newPrefab = Instantiate(lobbyPlayerListPrefab, lobbyPlayerListParent);
            newPrefab.GetComponentInChildren<TMP_Text>().text = player.Data["PlayerName"].Value;
        }
    }

    //subscribed events
    //lobby create or join failed
    private void LobbyAttemptSuccess(Lobby lobby)
    {
        ReenableUI();
    }
    private void LobbyAttemptFailed(LobbyServiceException e)
    {
        ReenableUI();
    }

    //get lobbies success
    private void GetLobbiesSuccess(QueryResponse queryResponse)
    {
        foreach (Transform child in lobbyParent)
            Destroy(child.gameObject);

        foreach (Lobby lobby in queryResponse.Results)
        {
            Button newPrefab = Instantiate(lobbyPrefab, lobbyParent);
            int lobbyPlayerCount = lobby.Players.Count;
            int lobbyMaxPlayerCount = lobby.MaxPlayers;
            newPrefab.GetComponentInChildren<TMP_Text>().text = $"{lobby.Name} ({lobbyPlayerCount} / {lobbyMaxPlayerCount}";
            newPrefab.onClick.AddListener(() => LobbyButtonAction(lobby));
        }
    }
}
