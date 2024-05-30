using Unity.Netcode;
using UnityEngine;

public abstract class WeaponPresenter : NetworkBehaviour, IWeapon
{
    [SerializeField] protected float cooldown = 0.5f;
    [SerializeField] protected float damageMultiplier = 1f;
    
    protected float NextUseCooldownTime;
    
    public virtual void UseWeapon(StatePayload callingState) {}
    
    public virtual void Equip() {}
    
    public virtual void Dequip() {}
    
    public void SetDamageMultiplier(float newDamageMultiplier)
    {
        damageMultiplier = newDamageMultiplier;
    }
}