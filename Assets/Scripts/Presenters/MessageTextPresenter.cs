using UnityEngine;

public class MessageTextPresenter : MonoBehaviour
{
    [SerializeField] private MessageTextUI view;
    [SerializeField] private Messages model;
    
    private Message _activeMessage;
    
    public void DisplayMessage(string text, float duration = 5)
    {
        model.AddMessage(text, duration);
    }
    
    private void Start()
    {
        model.OnPendingMessagesUpdated += MessagesUpdated;
    }
    
    private void OnDestroy()
    {
        model.OnPendingMessagesUpdated -= MessagesUpdated;
    }
    
    private void MessagesUpdated()
    {
        UpdateView();
    }
    
    private void UpdateView()
    {
        if (_activeMessage != null)
        {
            return;
        }
        
        var totalMessages = model.TotalMessages;
        
        if (totalMessages <= 0)
        {
            view.RemoveMessage();
            
            return;
        }
        
        _activeMessage = model.FirstMessage;
        
        view.DisplayMessage(_activeMessage);
        
        Invoke(nameof(RemoveActiveMessage), _activeMessage.Duration);
    }
    
    private void RemoveActiveMessage()
    {
        var currentMessage = _activeMessage;
        _activeMessage = null;
        
        model.RemoveMessage(currentMessage);
    }
}