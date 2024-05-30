using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLogic : NetworkBehaviour
{
    public static event Action<float> OnRoundTimerUpdated;
    public static event Action OnRoundOver;
    
    // Spawning
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private float roundLengthSeconds = 60;
    [SerializeField] private string welcomeMessage = "Welcome to the server!";
    [SerializeField] private string roundOverMessage = "Round is over!";
    [SerializeField] private float playerRespawnDelaySeconds = 3;
    
    private NetworkVariable<float> _roundEndTime = new(writePerm: NetworkVariableWritePermission.Server);
    private float _roundTimeLeft;
    private int _playSpawnRoundRobin;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // As a host we will have to welcome our local player to a spawn manually
        // since our own OnConnectionEvent gets triggered before we get to subscribe our GameLogic to it.
        if (IsHost)
            InitializeForPlayer(Player.LocalPlayer);

        // Server only logic.
        if (!IsServer)
            return;

        // We subscribe to the OnConnectionEvent event, so we can "welcome" players as soon as they join. 
        // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/connection-events/
        NetworkManager.OnConnectionEvent += OnConnectionEvent;
        
        StartRound();
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribing is important for the garbage collector.
        NetworkManager.OnConnectionEvent -= OnConnectionEvent;
        
        base.OnNetworkDespawn();
    }
    
    private void Update()
    {
        UpdateRoundTimer();
    }
    
    private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        var eventType = connectionEventData.EventType;
        var clientId = connectionEventData.ClientId;
        var playerList = Player.PlayerList;
        var player = playerList.GetValueOrDefault(clientId);
        
        if (!player)
        {
            return;
        }
        
        switch (eventType)
        {
            case ConnectionEvent.ClientConnected:
                InitializeForPlayer(player);
                
                break;
            case ConnectionEvent.ClientDisconnected:
                CleanupForPlayer(player);
                
                break;
            case ConnectionEvent.PeerConnected:
                break;
            case ConnectionEvent.PeerDisconnected:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void StartRound()
    {
        _roundEndTime.Value =  NetworkManager.Singleton.ServerTime.TimeAsFloat + roundLengthSeconds;
    }
    
    private void UpdateRoundTimer()
    {
        var currentTime = NetworkManager.Singleton.LocalTime.TimeAsFloat;
        _roundTimeLeft = Math.Max(_roundEndTime.Value - currentTime, 0);
        
        OnRoundTimerUpdated?.Invoke(_roundTimeLeft);
        
        if (_roundTimeLeft <= 0)
        {
            EndRound();
        }
    }
    
    private void EndRound()
    {
        OnRoundOver?.Invoke();
        
        if (!IsServer)
        {
            return;
        }
        
        var playerList = Player.PlayerList;
        var players = new List<Player>(playerList.Values);
        
        foreach (var player in players)
        {
            player.MessageRpc(roundOverMessage);
            StartCoroutine(SpawnPlayer(player));
        }
        
        StartRound();
    }

    private void InitializeForPlayer(Player player)
    {
        // Subscribe to his death events to handle kills.
        player.HealthPresenter.OnDeath += OnPlayerDeath;
        
        // Welcome our player to the server since we're so nice.
        player.MessageRpc(welcomeMessage);

        // Move him to a spawn position.
        StartCoroutine(SpawnPlayer(player));
    }
    
    private void CleanupForPlayer(Player player)
    {
        player.HealthPresenter.OnDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath(Player player)
    {
        var killer = player.HealthPresenter.LastDamageSource;

        if (killer)
        {
            var killerScore = killer.GetComponent<PlayerScorePresenter>();
            
            if (killerScore)
            {
                killerScore.Killed();
            }
            
            var killerName = killer.PlayerName.ToString();
            var victimName = player.PlayerName.ToString();
            
            Player.SendKillFeedUpdate(killerName, victimName);
            
            var playerGhost = killer.GetComponent<PlayerGhost>();
            
            if (playerGhost && playerGhost.IsAGhost)
            {
                killer.Restore();
            }
        }
        
        var playerScore = player.GetComponent<PlayerScorePresenter>();
        
        if (playerScore)
        {
            playerScore.Died();
        }
        
        StartCoroutine(SpawnPlayer(player, playerRespawnDelaySeconds));
    }
    
    private IEnumerator SpawnPlayer(Player player, float spawnDelay = 0)
    {
        yield return new WaitForSeconds(spawnDelay);
        
        player.SpawnAt(GetPlayerSpawn().position);
        
        _playSpawnRoundRobin++;
    }
    
    private Transform GetPlayerSpawn()
    {
        return playerSpawns[_playSpawnRoundRobin % playerSpawns.Length];
    }
}
