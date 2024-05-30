using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public event Action<float, float> HealthChanged;
    
    public float CurrentHealth => _currentHealth.Value;
    public float MinHealth => minHealth;
    public float MaxHealth => maxHealth;
    
    [SerializeField] private float minHealth;
    [SerializeField] private float maxHealth = 100;
    
    private NetworkVariable<float> _currentHealth = new(writePerm: NetworkVariableWritePermission.Server);
    
    public void Increment(float amount)
    {
        _currentHealth.Value += amount;
        _currentHealth.Value = Mathf.Clamp(_currentHealth.Value, minHealth, maxHealth);
    }
    
    public void Decrement(float amount)
    {
        _currentHealth.Value -= amount;
        _currentHealth.Value = Mathf.Clamp(_currentHealth.Value, minHealth, maxHealth);
    }
    
    public void Restore()
    {
        _currentHealth.Value = maxHealth;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _currentHealth.OnValueChanged += UpdateHealth;
        
        Restore();
    }
    
    public override void OnNetworkDespawn()
    {
        _currentHealth.OnValueChanged -= UpdateHealth;
        
        base.OnNetworkDespawn();
    }
    
    private void UpdateHealth(float previous, float current)
    {
        HealthChanged?.Invoke(previous, current);
    }
}