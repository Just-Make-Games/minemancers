using UnityEngine;

public class PlayerGhost : MonoBehaviour
{
    public bool IsAGhost => _isAGhost;
    
    [SerializeField] private HealthPresenter healthPresenter;
    [SerializeField] private MeleeWeaponPresenter meleeWeaponPresenter;
    [SerializeField] private RangedWeaponPresenter rangedWeaponPresenter;
    [SerializeField] private float regularDamageMultiplier = 1;
    [SerializeField] private float ghostDamageMultiplier = 2;
    
    private bool _isAGhost;
    
    private void Awake()
    {
        if (healthPresenter)
        {
            healthPresenter.OnUpdate += HealthUpdated;
        }
    }
    
    private void OnDestroy()
    {
        if (healthPresenter)
        {
            healthPresenter.OnUpdate -= HealthUpdated;
        }
    }
    
    private void HealthUpdated(float currentHealth, bool isGhostMode)
    {
        _isAGhost = isGhostMode;
        var damageMultiplier = _isAGhost ? ghostDamageMultiplier : regularDamageMultiplier;
        
        meleeWeaponPresenter.SetDamageMultiplier(damageMultiplier);
        rangedWeaponPresenter.SetDamageMultiplier(damageMultiplier);
    }
}