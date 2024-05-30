using System;
using Unity.Netcode;
using UnityEngine;

public enum WeaponID
{
    Pickaxe = 0,
    Ranged = 1
}

public class WeaponHandler : NetworkBehaviour
{
    public event Action OnWeaponChanged;
    
    [SerializeField] private Player player;
    [SerializeField] private CurrentWeaponUI currentWeaponUI;
    [SerializeField] private WeaponPresenter pickaxe;
    [SerializeField] private WeaponPresenter rangedWeapon;
    
    private NetworkVariable<WeaponID> _activeWeaponId = new(writePerm: NetworkVariableWritePermission.Server);
    private PlayerActions _playerActions;
    private IWeapon _activeWeapon;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _activeWeaponId.OnValueChanged += OnWeaponChange;
        _playerActions = player.PlayerHandlers.PlayerActions;
        _playerActions.onWeaponPrimaryDown += OnWeaponUse;
        
        SwitchWeapon();
    }
    
    public override void OnNetworkDespawn()
    {
        _playerActions.onWeaponPrimaryDown -= OnWeaponUse;
        _activeWeaponId.OnValueChanged -= OnWeaponChange;
        
        base.OnNetworkDespawn();
    }
    
    private void Update()
    {
        if (InputManager.Singleton.GetSwitchWeaponDown())
        {
            SwitchWeapon();
        }
    }
    
    private void SwitchWeapon()
    {
        if (!IsOwner) return;
        
        SwitchWeaponServerRPC();
    }
    
    [ServerRpc]
    private void SwitchWeaponServerRPC()
    {
        _activeWeaponId.Value = _activeWeaponId.Value switch
        {
            WeaponID.Pickaxe => WeaponID.Ranged,
            WeaponID.Ranged => WeaponID.Pickaxe,
            _ => throw new ArgumentOutOfRangeException(nameof(_activeWeaponId.Value), _activeWeaponId.Value,
                "Unknown active weapon id received")
        };
    }
    
    private void OnWeaponUse(StatePayload callingState)
    {
        _activeWeapon?.UseWeapon(callingState);
    }
    
    private void OnWeaponChange(WeaponID previous, WeaponID current)
    {
        _activeWeapon?.Dequip();
        
        string activeWeaponName;
        
        switch (current)
        {
            case WeaponID.Pickaxe:
                _activeWeapon = pickaxe;
                activeWeaponName = nameof(WeaponID.Pickaxe);
                
                break;
            case WeaponID.Ranged:
                _activeWeapon = rangedWeapon;
                activeWeaponName = nameof(WeaponID.Ranged);
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(current), current, "Unknown weapon found");
        }
        
        OnWeaponChanged?.Invoke();
        _activeWeapon.Equip();
        currentWeaponUI.UpdateWeaponText(activeWeaponName);
    }
}