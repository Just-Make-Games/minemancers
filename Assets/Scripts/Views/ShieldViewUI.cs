using UnityEngine;
using TMPro;

public class ShieldViewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text shieldText;
    
    public void UpdateShieldText(bool isActive, float duration)
    {
        if (!shieldText) return;
        
        var shieldStatusString = isActive ? "Active" : duration <= 0 ? "Inactive" : "Recharging";
        var durationString = duration.ToString("0.0");
        
        shieldText.text = $"Shield ({shieldStatusString}): {durationString}";
    }
}
