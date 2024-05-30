using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkedGameManager : MonoBehaviour
{
    public static NetworkedGameManager Singleton;
    public static LobbyInfoUpdated OnLobbyInfoUpdated;
    public static IsAwaitingCallback OnIsAwaitingCallback;
    public static LobbyAttemptSuccess OnLobbyAttemptSuccess;
    public static LobbyAttemptFailed OnLobbyAttemptFailed;
    public static GetLobbiesSuccessDelegate OnGetLobbiesSuccess;
    
    public delegate void LobbyInfoUpdated(Lobby lobby, bool isLobbyHost);
    public delegate void
        IsAwaitingCallback(bool isAwaitingCallback,
            string taskName); // Callback to UI elements when awaiting callback to block UI requests
    public delegate void LobbyAttemptSuccess(Lobby lobby);
    public delegate void LobbyAttemptFailed(LobbyServiceException e);
    public delegate void GetLobbiesSuccessDelegate(QueryResponse q);
    
    [SerializeField] private UnityTransport relayTransport;
    [SerializeField] private UnityTransport unityTransport;
    
    private const float HeartbeatInterval = 10f;
    private const float LobbyUpdateInterval = 2f;
    
    private bool IsLobbyHost { get; set; }
    private Lobby _joinedLobby;
    private float _nextHeartbeatTime;
    private float _nextLobbyUpdateTime;
    private bool _isGettingLobbies;
    private bool _isAwaitingCallback;
    
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Singleton = this;
        }
    }
    
    private void Start()
    {
        UGS.OnLobbyUpdateSuccess += LobbyInfoReceived;
        UGS.OnLobbyUpdateError += LobbyUpdateError;
        
        UGS.OnLobbyCreateSuccess += LobbyCreateSuccess;
        UGS.OnLobbyCreateFailed += LobbyCreateFailed;
        
        UGS.OnLobbyJoinSuccess += LobbyJoinSuccess;
        UGS.OnLobbyJoinFailed += LobbyJoinFailed;
        
        UGS.OnGetLobbiesSuccess += GetLobbiesSuccess;
        UGS.OnGetLobbiesFailed += GetLobbiesFailed;
        
        UGS.OnRelayCreateSuccess += RelayCreateSuccess;
        UGS.OnRelayCreateFailed += RelayCreateFailed;
        
        UGS.OnRelayJoinSuccess += RelayJoinSuccess;
        UGS.OnRelayJoinFailed += RelayJoinFailed;
    }
    
    private void Update()
    {
        // Actions only when we are in a lobby
        if (_joinedLobby == null)
            return;
        
        if (Time.time > _nextLobbyUpdateTime)
        {
            _nextLobbyUpdateTime = Time.time + LobbyUpdateInterval;
            UGS.UpdateLobby(_joinedLobby.Id);
        }
        
        //actions only when we're active host of a lobby
        if (!IsLobbyHost)
            return;
        
        if (Time.time > _nextHeartbeatTime)
        {
            _nextHeartbeatTime = Time.time + HeartbeatInterval;
            UGS.LobbyHeartbeat(_joinedLobby.Id);
        }
    }
    
    //----------------------------------LOCAL GAME Methods------------------------------------------------------
    public void CreateLocal()
    {
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
        NetworkManager.Singleton.StartHost();
    }
    
    public void ConnectToIP(string ip)
    {
        unityTransport.ConnectionData.Address = ip;
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
        NetworkManager.Singleton.StartClient();
    }
    
    //---------------------------------------LOBBY Methods-------------------------------------------------------
    public void CreateLobby()
    {
        UGS.CreateLobby();
    }
    
    public void JoinLobby(Lobby lobby)
    {
        UGS.JoinLobbyById(lobby.Id);
    }
    
    public void LeaveLobby()
    {
        UGS.LeaveLobby(_joinedLobby.Id);
    }
    
    public void JoinLobbyByCode(string code)
    {
        UGS.JoinLobbyByCode(code);
    }
    
    public void KickPlayerInLobby(string playerId)
    {
        UGS.KickPlayerInLobby(_joinedLobby.Id, playerId);
    }
    
    public void GetLobbies()
    {
        if (_isGettingLobbies)
            return;
        
        _isGettingLobbies = true;
        UGS.GetLobbies();
    }
    
    public void StartRelayGame()
    {
        if (!IsLobbyHost || _isAwaitingCallback)
            return;
        
        UGS.CreateRelay();
        SetAwaitingCallback(true, "Starting game...");
    }
    
    private void JoinRelayGame(string relayCode)
    {
        if (_isAwaitingCallback)
            return;
        
        SetAwaitingCallback(true, "Joining game...");
        
        UGS.JoinRelay(relayCode);
        LeaveLobby();
    }
    
    private void SetAwaitingCallback(bool awaiting, string reason = "")
    {
        _isAwaitingCallback = awaiting;
        OnIsAwaitingCallback(_isAwaitingCallback, reason);
    }
    
    
    //-------------------------------------LOBBY logic-----------------------------------------------------------------
    private void LobbyInfoReceived(Lobby lobby)
    {
        // Update our lobby info
        _joinedLobby = lobby;
        
        // Check whether we're still in the lobby
        if (!IsPlayerInLobby(_joinedLobby))
        {
            // If not then already update the info and return
            ResetLobbyInfo(false);
            OnLobbyInfoUpdated?.Invoke(_joinedLobby, IsLobbyHost);
            
            return;
        }
        
        // Else check for additional data. the order of checking is important
        if (!IsLobbyHost &&
            HasLobbyStartedGame(
                _joinedLobby)) // If lobby has started game we want to join to its relay code and leave the lobby
        {
            JoinRelayGame(_joinedLobby.Data[UGS.KeyRelayCode].Value);
            return;
        }
        
        IsLobbyHost = IsPlayerTheLobbyHost(_joinedLobby);
        
        OnLobbyInfoUpdated?.Invoke(_joinedLobby, IsLobbyHost);
    }
    
    private bool IsPlayerInLobby(Lobby lobby)
    {
        return lobby?.Players != null && lobby.Players.Any(player => player.Id == AuthenticationService.Instance.PlayerId);
    }
    
    private bool IsPlayerTheLobbyHost(Lobby lobby)
    {
        return lobby?.Players != null && lobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    
    private bool HasLobbyStartedGame(Lobby lobby)
    {
        var relayCode = lobby.Data[UGS.KeyRelayCode].Value;
        
        return relayCode != "0"; // If the relay code is not the default "0" value it means the game has been started
    }
    
    private void LobbyUpdateError(LobbyServiceException e)
    {
        if (e.Reason != LobbyExceptionReason.LobbyNotFound) return;
        
        // The lobby we were trying to update was not found. This means it's not there anymore
        ResetLobbyInfo();
    }
    
    private void ResetLobbyInfo(bool sendOnLobbyInfoUpdated = true)
    {
        _joinedLobby = null;
        IsLobbyHost = false;
        
        if (!sendOnLobbyInfoUpdated) return;
        
        OnLobbyInfoUpdated?.Invoke(_joinedLobby, IsLobbyHost);
    }
    
    //---------------------------------------CALLBACKS----------------------------------------------
    private void LobbyCreateSuccess(Lobby lobby)
    {
        OnLobbyAttemptSuccess?.Invoke(lobby);
        LobbyInfoReceived(lobby);
    }
    
    private void LobbyCreateFailed(LobbyServiceException e)
    {
        OnLobbyAttemptFailed?.Invoke(e);
    }
    
    private void LobbyJoinSuccess(Lobby lobby)
    {
        OnLobbyAttemptSuccess?.Invoke(lobby);
        LobbyInfoReceived(lobby);
    }
    
    private void LobbyJoinFailed(LobbyServiceException e)
    {
        OnLobbyAttemptFailed?.Invoke(e);
    }
    
    private void GetLobbiesSuccess(QueryResponse q)
    {
        _isGettingLobbies = false;
        OnGetLobbiesSuccess?.Invoke(q);
    }
    
    private void GetLobbiesFailed(LobbyServiceException e)
    {
        _isGettingLobbies = false;
    }
    
    private void RelayCreateSuccess(Allocation allocation, string joinCode)
    {
        SetAwaitingCallback(false);
        
        UGS.SetLobbyInfoRelayCodeAndLeave(_joinedLobby.Id, joinCode);
        
        RelayServerData relayServerData = new(allocation, "dtls");
        
        relayTransport.SetRelayServerData(relayServerData);
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;
        NetworkManager.Singleton.StartHost();
    }
    
    private void RelayCreateFailed(RelayServiceException e)
    {
        SetAwaitingCallback(false);
    }
    
    private void RelayJoinSuccess(JoinAllocation allocation)
    {
        SetAwaitingCallback(false);
        
        var relayServerData = new RelayServerData(allocation, "dtls");
        
        relayTransport.SetRelayServerData(relayServerData);
        
        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;
        NetworkManager.Singleton.StartClient();
    }
    
    private void RelayJoinFailed(System.Exception e)
    {
        SetAwaitingCallback(false);
    }
}