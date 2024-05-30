using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    
    public void UpdateTime(float timeLeft)
    {
        var minutes = Mathf.FloorToInt(timeLeft / 60F);
        var seconds = Mathf.FloorToInt(timeLeft - minutes * 60);
        var formattedTime = $"{minutes:0}:{seconds:00}";
        
        timeText.text = formattedTime;
    }
}
