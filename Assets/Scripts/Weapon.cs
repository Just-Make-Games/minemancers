using UnityEngine;
using Unity.Netcode;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Impact collisionImpactPrefab;
    [SerializeField] private Player source;
    [SerializeField] private float damage = 10;
    [SerializeField] private float damageMultiplier = 1;
    
    private Vector3 _attackStartPosition;
    
    public void SetAttackStartPosition(Vector3 attackStartPosition)
    {
        _attackStartPosition = attackStartPosition;
    }
    
    public void SetSource(Player newSource)
    {
        source = newSource;
    }
    
    public void SetDamageMultiplier(float newDamageMultiplier)
    {
        damageMultiplier = newDamageMultiplier;
    }
    
    protected virtual void DealDamage(IDamageable damageable)
    {
        if (!NetworkManager.Singleton.IsServer || damageable == null) return;
        
        damageable.TakeDamage(damage * damageMultiplier, source);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var damageable = other.GetComponent<IDamageable>();
        
        if (damageable == null)
        {
            var direction = transform.position - _attackStartPosition;
            var rotation = Quaternion.LookRotation(Vector3.back, direction);
            
            Instantiate(collisionImpactPrefab, transform.position, rotation);
        }
        
        DealDamage(damageable);
    }

    public void CollidedWith(Transform other)
    {
        var damageable = other.GetComponent<IDamageable>();
        
        DealDamage(damageable);
    }
}
