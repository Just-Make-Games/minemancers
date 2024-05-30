using System.Globalization;
using TMPro;
using UnityEngine;

public class WeaponViewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    
    public void UpdateAmmoTextValue(float currentValue, float maxValue)
    {
        if (!ammoText) return;
        
        var currentValueString = currentValue.ToString(CultureInfo.InvariantCulture);
        var maxValueString = maxValue.ToString(CultureInfo.InvariantCulture);
        
        ammoText.text = $"{currentValueString} / {maxValueString}";
    }
}