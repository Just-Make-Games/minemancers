using System;

public class Message
{
    public Guid Id => _id;
    public string Text => _text;
    public float Duration => _duration;
    
    private Guid _id;
    private string _text;
    private float _duration;
    
    public Message(Guid id, string text, float duration)
    {
        _id = id;
        _text = text;
        _duration = duration;
    }
}