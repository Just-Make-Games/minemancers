using UnityEngine;
using TMPro;

public class MessageTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    
    public void DisplayMessage(Message message)
    {
        infoText.gameObject.SetActive(true);
        
        infoText.text = message.Text;
    }
    
    public void RemoveMessage()
    {
        infoText.text = "";
        
        infoText.gameObject.SetActive(false);
    }
}