using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;


public class Player : NetworkBehaviour
{
    public static Dictionary<ulong, Player> PlayerList = new();
    public static Player LocalPlayer;
    
    public static void SendKillFeedUpdate(string killerName, string victimName)
    {
        foreach (var (_, player) in PlayerList)
        {
            player.KilledRpc(killerName, victimName);
        }
    }
    
    public FixedString64Bytes PlayerName => _name.Value;
    public PlayerHandlers PlayerHandlers => playerHandlers;
    public HealthPresenter HealthPresenter => healthPresenter;
    
    [SerializeField] private HealthPresenter healthPresenter;
    [SerializeField] private RangedWeaponPresenter rangedWeaponPresenter;
    [SerializeField] private ShieldPresenter shieldPresenter;
    [SerializeField] private PlayerStateManager playerStateManager;
    [SerializeField] private Canvas playerUI;
    [SerializeField] private PlayerNameViewUI playerNameViewUI;
    [SerializeField] private MessageTextPresenter playerMessages;
    [SerializeField] private KillFeedPresenter playerKillFeed;
    [SerializeField] private PlayerHandlers playerHandlers;
    
    private NetworkVariable<FixedString64Bytes> _name = new("Player");
    private NetworkVariable<FixedString64Bytes> _id = new("-");
    
    
    public override void OnNetworkSpawn()
    {
        if (!PlayerList.ContainsKey(OwnerClientId))
            PlayerList.Add(OwnerClientId, this);
        else
            Debug.LogError("Our Player list already contains a player with this id");
        
        _name.OnValueChanged += OnPlayerNameChange;
        
        if (IsOwner)
        {
            var playerName = MenuUI.PlayerName;
            
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Nameless";
            }
            
            var playerId = AuthenticationService.Instance.PlayerId;
            
            InitializeDataServerRpc(playerName, playerId);
            
            LocalPlayer = this;
            
            playerUI?.gameObject.SetActive(true);
        }
        
        playerHandlers.Initialize(playerStateManager);
        
        UpdatePlayerNameView(_name.Value.ToString());
    }
    
    public override void OnNetworkDespawn()
    {
        _name.OnValueChanged -= OnPlayerNameChange;
        
        PlayerList.Remove(OwnerClientId);
    }
    
    public void SpawnAt(Vector3 position)
    {
        MoveToPosition(position);
        Restore();
    }
    
    public void Restore()
    {
        healthPresenter.Restore();
        rangedWeaponPresenter.Restore();
        shieldPresenter.Restore();
    }
    
    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
    public void MessageRpc(string text, float duration = 5f)
    {
        playerMessages.DisplayMessage(text, duration);
    }

    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
    public void KilledRpc(string killerName, string victimName)
    {
        var killMessage = $"{killerName} killed {victimName}";
        
        playerKillFeed.DisplayMessage(killMessage, 10);
    }
    
    private void MoveToPosition(Vector3 position)
    {
        playerStateManager.ForceSetPosition(position);
    }
    
    [ServerRpc]
    private void InitializeDataServerRpc(string playerName, string playerId)
    {
        _name.Value = playerName;
        _id.Value = playerId;
    }
    
    private void OnPlayerNameChange(FixedString64Bytes previousName, FixedString64Bytes currentName)
    {
        UpdatePlayerNameView(currentName.ToString());
    }
    
    private void UpdatePlayerNameView(string newPlayerName)
    {
        playerNameViewUI.UpdateText(newPlayerName);
    }
}
