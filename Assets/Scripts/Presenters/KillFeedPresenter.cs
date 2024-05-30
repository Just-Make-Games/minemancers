using System.Collections;
using UnityEngine;

public class KillFeedPresenter : MonoBehaviour
{
    [SerializeField] private KillFeedViewUI view;
    [SerializeField] private Messages model;
    
    public void DisplayMessage(string text, float duration = 5)
    {
        model.AddMessage(text, duration);
    }
    
    private void Start()
    {
        model.OnMessageAdded += MessageAdded;
        model.OnMessageRemoved += MessageRemoved;
    }
    
    private void OnDestroy()
    {
        model.OnMessageRemoved -= MessageRemoved;
        model.OnMessageAdded -= MessageAdded;
    }
    
    private void MessageAdded(Message message)
    {
        view.DisplayMessage(message);
        
        StartCoroutine(RemoveMessage(message));
    }
    
    private void MessageRemoved(Message message)
    {
        view.RemoveMessage(message);
    }
    
    private IEnumerator RemoveMessage(Message message)
    {
        yield return new WaitForSeconds(message.Duration);
        
        model.RemoveMessage(message);
    }
}