using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillFeedViewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text killFeedItemPrefab;
    [SerializeField] private GameObject contentParent;
    
    private Dictionary<Message, TMP_Text> _killFeedItemsByMessage;
    
    public void DisplayMessage(Message message)
    {
        var killFeedItem = Instantiate(killFeedItemPrefab, contentParent.transform);
        
        killFeedItem.transform.SetSiblingIndex(0);
        
        killFeedItem.text = message.Text;
        
        _killFeedItemsByMessage[message] = killFeedItem;
    }
    
    public void RemoveMessage(Message message)
    {
         if (!_killFeedItemsByMessage.TryGetValue(message, out var killFeedItem)) {
             return;
         }
         
         Destroy(killFeedItem);
         _killFeedItemsByMessage.Remove(message);
    }
    
    protected void Start()
    {
        _killFeedItemsByMessage = new Dictionary<Message, TMP_Text>();
    }
}