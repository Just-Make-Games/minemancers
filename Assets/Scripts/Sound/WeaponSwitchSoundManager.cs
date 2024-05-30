using UnityEngine;

public class WeaponSwitchSoundManager : SoundManager
{
    [SerializeField] private WeaponHandler weaponHandler;
    
    private void Awake()
    {
        weaponHandler.OnWeaponChanged += PlayRandomSound;
    }
    
    private void OnDestroy()
    {
        weaponHandler.OnWeaponChanged -= PlayRandomSound;
    }
}
