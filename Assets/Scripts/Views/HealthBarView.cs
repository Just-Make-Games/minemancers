using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarView : HealthView
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    
    protected override void InitializeValue(float minValue, float maxValue)
    {
        SetMinSliderValue(minValue);
        SetMaxSliderValue(maxValue);
    }
    
    protected override void UpdateValue(float value, bool isGhostMode)
    {
        UpdateSliderValue(value);
        UpdateTextValue(value);
    }
    
    private void SetMinSliderValue(float value)
    {
        if (!healthSlider) return;
        
        healthSlider.minValue = value;
    }
    
    private void SetMaxSliderValue(float value)
    {
        if (!healthSlider) return;
        
        healthSlider.maxValue = value;
    }
    
    private void UpdateSliderValue(float value)
    {
        if (!healthSlider) return;
        
        healthSlider.value = value;
    }
    
    private void UpdateTextValue(float value)
    {
        if (!healthText) return;
        
        healthText.text = value.ToString(CultureInfo.InvariantCulture);
    }
}