using System.Globalization;
using UnityEngine;
using TMPro;

public class HealthViewUI : HealthView
{
    [SerializeField] private TMP_Text healthText;
    
    protected override void InitializeValue(float minValue, float maxValue)
    {
    }
    
    protected override void UpdateValue(float value, bool isGhostMode)
    {
        var valueString = value.ToString(CultureInfo.InvariantCulture);
        var ghostModeString = isGhostMode ? " (Ghost)" : "";
        
        healthText.text = $"Health: {valueString}" + ghostModeString;
    }
}