public class MeleeWeapon : Weapon
{
    protected override void DealDamage(IDamageable damageable)
    {
        base.DealDamage(damageable);
        gameObject.SetActive(false);
    }
}