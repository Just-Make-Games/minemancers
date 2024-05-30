using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    public static event Action<Player, int> OnKill;
    public event Action<int, int> OnScoreChanged;
    
    public int Kills => _kills.Value;
    public int Deaths => _deaths.Value;
    
    [SerializeField] private Player player;
    
    private NetworkVariable<int> _kills = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _deaths = new(writePerm: NetworkVariableWritePermission.Server);
    
    public void AddKillCount(int increment = 1)
    {
        _kills.Value += increment;
    }
    
    public void AddDeathCount(int increment = 1)
    {
        _deaths.Value += increment;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _kills.OnValueChanged += KillsChanged;
        _deaths.OnValueChanged += DeathsChanged;
        
        GameLogic.OnRoundOver += Reset;
        
        Reset();
    }
    
    public override void OnNetworkDespawn()
    {
        _kills.OnValueChanged -= KillsChanged;
        _deaths.OnValueChanged -= DeathsChanged;
        
        GameLogic.OnRoundOver -= Reset;
        
        base.OnNetworkDespawn();
    }
    
    private void KillsChanged(int previousKills, int currentKills)
    {
        ScoreChanged(currentKills, _deaths.Value);
        
        OnKill?.Invoke(player, currentKills);
    }
    
    private void DeathsChanged(int previousDeaths, int currentDeaths)
    {
        ScoreChanged(_kills.Value, currentDeaths);
    }
    
    private void ScoreChanged(int kills, int deaths)
    {
        OnScoreChanged?.Invoke(kills, deaths);
    }
    
    private void Reset()
    {
        if (!IsServer)
        {
            return;
        }
        
        _kills.Value = 0;
        _deaths.Value = 0;
    }
}
