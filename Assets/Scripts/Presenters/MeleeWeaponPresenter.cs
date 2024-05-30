using UnityEngine;

public class MeleeWeaponPresenter : WeaponPresenter
{
    [SerializeField] private MeleeWeapon meleeWeapon;
    [SerializeField] private float attackDuration = 0.1f;
    
    private float _attackDurationTime;
    
    public override void UseWeapon(StatePayload callingState)
    {
        if (!meleeWeapon || Time.time <= NextUseCooldownTime)
        {
            return;
        }
        
        NextUseCooldownTime = Time.time + cooldown;
        
        StartAttack();
    }
    
    private void Update()
    {
        StopAttack();
    }
    
    private void StartAttack()
    {
        if (!meleeWeapon)
        {
            return;
        }
        
        meleeWeapon.SetDamageMultiplier(damageMultiplier);
        
        var weaponGameObject = meleeWeapon.gameObject;
        
        if (weaponGameObject.activeInHierarchy)
        {
            return;
        }
        
        _attackDurationTime = Time.time + attackDuration;
        
        meleeWeapon.SetAttackStartPosition(transform.position);
        weaponGameObject.SetActive(true);
    }
    
    private void StopAttack()
    {
        if (!meleeWeapon || Time.time < _attackDurationTime)
        {
            return;
        }
        
        var weaponGameObject = meleeWeapon.gameObject;
        
        if (!weaponGameObject.activeInHierarchy)
        {
            return;
        }
        
        weaponGameObject.SetActive(false);
    }
}