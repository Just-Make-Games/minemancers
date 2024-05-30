using Unity.Netcode;
using UnityEngine;

public class Ammo : NetworkBehaviour
{
    [SerializeField] private int amount = 1;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var rangedWeapon = other.GetComponent<RangedWeaponPresenter>();
        
        if (!rangedWeapon)
        {
            return;
        }
        
        if (!IsServer) return;
        
        rangedWeapon.AddAmmo(amount);
        
        var networkObject = GetComponent<NetworkObject>();
        
        networkObject.Despawn();
    }
}
