using System;
using Unity.Netcode;
using UnityEngine;

public class AmmoMag : NetworkBehaviour
{
    public event Action OnAmmoChanged;
    public int CurrentAmmo => _currentAmmo.Value;
    public int MaxAmmo => maxAmmo;
    
    [SerializeField] private int maxAmmo = 10;
    
    private const int MinAmmo = 0;
    
    private NetworkVariable<int> _currentAmmo = new (writePerm: NetworkVariableWritePermission.Server);
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _currentAmmo.OnValueChanged += AmmoChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        _currentAmmo.OnValueChanged -= AmmoChanged;
        
        base.OnNetworkDespawn();
    }
    
    public void AddAmmo(int amount)
    {
        if (_currentAmmo.Value + amount > maxAmmo)
        {
            return;
        }
        
        _currentAmmo.Value += amount;
    }
    
    public bool RemoveAmmo(int amount)
    {
        if (_currentAmmo.Value - amount < MinAmmo)
        {
            return false;
        }
        
        if (IsServer)
        {
            _currentAmmo.Value -= amount;
        }
        
        return true;
    }
    
    public void Restore()
    {
        _currentAmmo.Value = maxAmmo;
    }
    
    private void AmmoChanged(int previous, int current)
    {
        OnAmmoChanged?.Invoke();
    }
}
