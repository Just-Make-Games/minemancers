using System.Globalization;
using UnityEngine;
using TMPro;

public class HighScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;
    
    public void UpdateScore(string playerName, int score)
    {
        var formattedScore = score.ToString(CultureInfo.InvariantCulture);
        
        playerNameText.text = playerName;
        scoreText.text = $"({formattedScore})";
    }
    
    public void Reset()
    {
        playerNameText.text = "None";
        scoreText.text = "(-)";
    }
}
