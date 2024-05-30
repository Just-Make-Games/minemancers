using System;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int IsDead = Animator.StringToHash("IsDead");
    
    public event Action Step;
    public event Action Attack;
    
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private HealthPresenter playerHealthPresenter;
    [SerializeField] private WeaponHandler playerWeaponHandler;
    
    private bool _isFacingRight = true;
    private bool _isDead;
    
    public void OnStep()
    {
        Step?.Invoke();
    }
    
    public void OnAttack()
    {
        Attack?.Invoke();
    }
    
    public void Initialize()
    {
        playerHealthPresenter.OnDeath += OnPlayerDeath;
        playerHealthPresenter.OnRestore += OnPlayerRestore;
    }
    
    public void ParseState(StatePayload payload)
    {
        if (_isDead)
        {
            playerAnimator.SetBool(IsMoving, false);
            playerAnimator.SetBool(IsAttacking, false);
            
            return;
        }
        
        var movementInput = payload.MovementInput;
        var isMoving = movementInput != Vector2.zero;
        var isAttacking = payload.WeaponPrimaryDown;
        var inputX = movementInput.x;
        _isFacingRight = inputX switch
        {
            > 0 => true,
            < 0 => false,
            _ => _isFacingRight
        };
        playerSprite.flipX = !_isFacingRight;
        
        playerAnimator.SetBool(IsMoving, isMoving);
        playerAnimator.SetBool(IsAttacking, isAttacking);
    }
    
    private void OnPlayerDeath(Player player)
    {
        _isDead = true;
        
        playerAnimator.SetBool(IsDead, true);
    }
    
    private void OnPlayerRestore()
    {
        _isDead = false;
        
        playerAnimator.SetBool(IsDead, false);
    }
}