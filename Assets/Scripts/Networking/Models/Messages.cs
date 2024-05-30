using System;
using System.Collections.Generic;
using UnityEngine;

public class Messages : MonoBehaviour
{
    public event Action OnPendingMessagesUpdated;
    public event Action<Message> OnMessageAdded;
    public event Action<Message> OnMessageRemoved; 
    public int TotalMessages => _pendingMessages.Count;
    public Message FirstMessage => _pendingMessages.Count > 0 ? _pendingMessages[0] : null;
    
    private List<Message> _pendingMessages;
    
    public void AddMessage(string text, float duration)
    {
        var newMessage = new Message(new Guid(), text, duration);
        
        _pendingMessages.Add(newMessage);
        OnMessageAdded?.Invoke(newMessage);
        OnPendingMessagesUpdated?.Invoke();
    }
    
    public void RemoveMessage(Message message)
    {
        _pendingMessages.Remove(message);
        OnMessageRemoved?.Invoke(message);
        OnPendingMessagesUpdated?.Invoke();
    }
    
    private void Start()
    {
        _pendingMessages = new List<Message>();
    }
}