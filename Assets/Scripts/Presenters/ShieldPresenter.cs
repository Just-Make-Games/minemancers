using System;
using UnityEngine;

public class ShieldPresenter : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private ShieldView view;
    [SerializeField] private ShieldViewUI viewUI;
    [SerializeField] private float shieldCooldown = 3.0f;
    [SerializeField] private float shieldDuration = 2.0f;
    
    private PlayerActions _playerActions;
    private float _nextShieldCooldownTime;
    private float _shieldDurationTime;
    
    public void Restore()
    {
        _nextShieldCooldownTime = 0;
        _shieldDurationTime = 0;
        
        DeactivateShield();
    }
    
    private void Start()
    {
        if (!player) return;
        
        _playerActions = player.PlayerHandlers?.PlayerActions;
        
        if (_playerActions == null) return;
        
        _playerActions.onWeaponSecondaryDown += ActivateShield;
    }
    
    private void Update()
    {
        DeactivateShield();
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (_playerActions == null) return;
        
        _playerActions.onWeaponSecondaryDown -= ActivateShield;
    }
    
    private void ActivateShield()
    {
        if (Time.time < _nextShieldCooldownTime)
        {
            return;
        }
        
        var isShieldEnabled = view.EnableShield();
        
        if (!isShieldEnabled)
        {
            return;
        }
        
        _nextShieldCooldownTime = Time.time + shieldCooldown;
        _shieldDurationTime = Time.time + shieldDuration;
    }
    
    private void DeactivateShield()
    {
        if (IsActive())
        {
            return;
        }
        
        view.DisableShield();
    }
    
    private bool IsActive()
    {
        return Time.time < _shieldDurationTime;
    }
    
    private void UpdateUI()
    {
        var isShieldActive = IsActive();
        var currentTime = Time.time;
        var cooldownDuration = _nextShieldCooldownTime - currentTime;
        var currentShieldDuration = _shieldDurationTime - currentTime;
        var duration = Math.Max(isShieldActive ? currentShieldDuration : cooldownDuration, 0);
        
        viewUI.UpdateShieldText(isShieldActive, duration);
    }
}
