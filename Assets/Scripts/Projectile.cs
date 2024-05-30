using UnityEngine;

public class Projectile : Weapon
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float maxLifeTime = 5f;
    [SerializeField] private float speed = 10f;
    public float Speed => speed;

    private void Awake()
    {
        Destroy(gameObject, maxLifeTime);
    }
    
    private void Start()
    {
        Launch();
    }
    
    private void Launch()
    {
        rb.velocity = transform.right * speed;
    }
    
    protected override void DealDamage(IDamageable damageable)
    {
        base.DealDamage(damageable);
        Destroy(gameObject);
    }
}
