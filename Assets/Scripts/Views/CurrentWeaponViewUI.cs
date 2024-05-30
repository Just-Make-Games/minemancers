using UnityEngine;
using TMPro;

public class CurrentWeaponUI : MonoBehaviour
{
    [SerializeField] private TMP_Text weaponText;
    
    public void UpdateWeaponText(string weaponName)
    {
        if (!weaponText) return;
        
        weaponText.text = weaponName;
    }
}
