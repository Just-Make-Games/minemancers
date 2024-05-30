using System;
using UnityEngine;

[Serializable]
public class PlayerHandlers
{
    public PlayerActions PlayerActions => _playerActions;
    
    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private HealthPresenter playerHealthPresenter;
    
    private PlayerActions _playerActions;
    private bool _canAct = true;
    
    public void Initialize(PlayerStateManager playerStateManager)
    {
        _playerActions = new PlayerActions();
        
        playerAnimator.Initialize();
        
        playerStateManager.OnUpdateWithState += Update;
        playerHealthPresenter.OnDeath += OnPlayerDeath;
        playerHealthPresenter.OnRestore += OnPlayerRestore;
    }

    private void Update(StatePayload state)
    {
        playerAnimator.ParseState(state);
        
        if (_canAct)
        {
            _playerActions.Update(state);
        }
    }
    
    private void OnPlayerDeath(Player player)
    {
        _canAct = false;
    }
    
    private void OnPlayerRestore()
    {
        _canAct = true;
    }
}
