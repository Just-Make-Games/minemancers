using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class UGS : MonoBehaviour
{
    public static LobbyUpdateSuccess OnLobbyUpdateSuccess;
    public static LobbyUpdateError OnLobbyUpdateError;
    public static LobbyJoinSuccess OnLobbyCreateSuccess;
    public static LobbyJoinFailed OnLobbyCreateFailed;
    public static LobbyJoinSuccess OnLobbyJoinSuccess;
    public static LobbyJoinFailed OnLobbyJoinFailed;
    public static GetLobbiesSuccess OnGetLobbiesSuccess;
    public static GetLobbiesFailed OnGetLobbiesFailed;
    public static RelayCreateSuccess OnRelayCreateSuccess;
    public static RelayCreateFailed OnRelayCreateFailed;
    public static RelayJoinSuccess OnRelayJoinSuccess;
    public static RelayJoinFailed OnRelayJoinFailed;
    
    public const string KeyRelayCode = "RelayCode";
    
    public delegate void LobbyUpdateSuccess(Lobby lobby);
    public delegate void LobbyUpdateError(LobbyServiceException lobby);
    public delegate void LobbyCreateSuccess(Lobby lobby);
    public delegate void LobbyCreateFailed(LobbyServiceException e);
    public delegate void LobbyJoinSuccess(Lobby lobby);
    public delegate void LobbyJoinFailed(LobbyServiceException e);
    public delegate void GetLobbiesSuccess(QueryResponse queryResponse);
    public delegate void GetLobbiesFailed(LobbyServiceException e);
    public delegate void RelayCreateSuccess(Allocation allocation, string joinCode);
    public delegate void RelayCreateFailed(RelayServiceException e);
    public delegate void RelayJoinSuccess(JoinAllocation allocation);
    public delegate void RelayJoinFailed(Exception e);
    
    private void Start()
    {
        InitializeUgs();
    }
    
    private async void InitializeUgs()
    {
        var options = new InitializationOptions();
        options.SetProfile("profile" + UnityEngine.Random.Range(0, 1000));
        
        await UnityServices.InitializeAsync(options);
        AuthenticationService.Instance.SignedIn +=
            () => Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    
    #region Relay
    
    public static async void CreateRelay(int maxPlayers = 3)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            OnRelayCreateSuccess(allocation, joinCode);
        }
        catch (RelayServiceException e)
        {
            OnRelayCreateFailed(e);
        }
    }
    
    public static async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log($"Joining Relay with {joinCode}");
            
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            OnRelayJoinSuccess(allocation);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            OnRelayJoinFailed(e);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            OnRelayJoinFailed(e);
        }
    }
    
    #endregion
    
    
    #region Lobby
    
    public static async void CreateLobby()
    {
        try
        {
            var maxPlayers = 4;
            var lobbyName = $"{MenuUI.PlayerName}'s Lobby";
            
            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { KeyRelayCode, new DataObject(DataObject.VisibilityOptions.Member, "0") }
                },
                Player = GetPlayer(),
            };
            
            var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            
            OnLobbyCreateSuccess?.Invoke(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnLobbyCreateFailed?.Invoke(e);
        }
    }
    
    public static async void GetLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new()
            {
                Count = 60,
                Filters = new List<QueryFilter>
                    { new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder> { new(false, QueryOrder.FieldOptions.Created) }
            };
            var queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            
            OnGetLobbiesSuccess?.Invoke(queryResponse);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnGetLobbiesFailed?.Invoke(e);
        }
    }
    
    public static async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions options = new()
            {
                Player = GetPlayer()
            };
            
            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            
            OnLobbyJoinSuccess?.Invoke(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnLobbyJoinFailed?.Invoke(e);
        }
    }
    
    public static async void JoinLobbyById(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new()
            {
                Player = GetPlayer()
            };
            
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);
            
            OnLobbyJoinSuccess?.Invoke(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnLobbyJoinFailed?.Invoke(e);
        }
    }
    
    public static async void LeaveLobby(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId);
            UpdateLobby(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    public static async void KickPlayerInLobby(string lobbyId, string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private async void DeleteLobby(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    public static async void LobbyHeartbeat(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    public static async void UpdateLobby(string lobbyId)
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
            
            OnLobbyUpdateSuccess?.Invoke(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            
            OnLobbyUpdateError?.Invoke(e);
        }
    }
    
    public static async void SetLobbyInfoRelayCodeAndLeave(string lobbyId, string relayCode)
    {
        try
        {
            UpdateLobbyOptions updateLobbyOptions = new()
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KeyRelayCode, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                },
            };
            
            var lobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateLobbyOptions);
            
            LeaveLobby(lobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private static Unity.Services.Lobbies.Models.Player GetPlayer()
    {
        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                { { "PlayerName", new(PlayerDataObject.VisibilityOptions.Public, MenuUI.PlayerName) } }
        };
    }
    
    #endregion
}