using System.Globalization;
using UnityEngine;
using TMPro;

public class ScoreViewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;
    
    public void UpdateText(int kills, int deaths)
    {
        var killsString = $"Kills: {kills.ToString(CultureInfo.InvariantCulture)}";
        var deathsString = $"Deaths: {deaths.ToString(CultureInfo.InvariantCulture)}";
        
        killsText.text = killsString;
        deathsText.text = deathsString;
    }
}