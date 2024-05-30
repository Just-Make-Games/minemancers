using UnityEngine;
using TMPro;

public class PlayerNameViewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    
    public void UpdateText(string playerName)
    {
        playerNameText.text = playerName;
    }
}