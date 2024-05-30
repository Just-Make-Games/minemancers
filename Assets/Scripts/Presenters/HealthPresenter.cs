using System;
using Unity.Netcode;
using UnityEngine;

public class HealthPresenter : NetworkBehaviour, IDamageable
{
    public event Action<float, float> OnInitialize; 
    public event Action<float, bool> OnUpdate;
    public event Action OnHurt;
    public event Action<Player> OnDeath;
    public event Action OnRestore;
    public Player LastDamageSource => _lastDamageSource;
    
    [SerializeField] private Player player;
    [SerializeField] private Health health;
    [SerializeField] private float lastDamageSourceThreshold = 10f;
    
    private Player _lastDamageSource;
    private bool _isDead;
    
    public void TakeDamage(float amount, Player source)
    {
        if (_isDead)
        {
            return;
        }
        
        if (source && health)
        {
            var currentHealth = health.CurrentHealth;
            var newHealth = currentHealth - amount;
            
            if (newHealth >= lastDamageSourceThreshold)
            {
                _lastDamageSource = source;
            }
        }
        
        health?.Decrement(amount);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (health)
        {
            health.HealthChanged += OnHealthChanged;
            
            OnInitialize?.Invoke(health.MinHealth, health.MaxHealth);
        }
        
        UpdateView();
    }
    
    public override void OnNetworkDespawn()
    {
        if (health)
        {
            health.HealthChanged -= OnHealthChanged;
        }
        
        base.OnNetworkDespawn();
    }
    
    public void Restore()
    {
        health?.Restore();
        
        _isDead = false;
    }
    
    private void OnHealthChanged(float previous, float current)
    {
        UpdateView();
        
        var minHealth = health.MinHealth;
        
        if (current <= minHealth)
        {
            _isDead = true;
            
            OnDeath?.Invoke(player);
        } else if (previous <= minHealth && current > minHealth)
        {
            OnRestore?.Invoke();
        } else if (current < previous)
        {
            OnHurt?.Invoke();
        }
    }
    
    private void UpdateView()
    {
        if (!health || health.MaxHealth == 0)
        {
            return;
        }
        
        var currentHealth = health.CurrentHealth;
        var isHealthBelowThreshold = currentHealth > health.MinHealth && currentHealth <= lastDamageSourceThreshold;
        
        OnUpdate?.Invoke(currentHealth, isHealthBelowThreshold);
    }
}