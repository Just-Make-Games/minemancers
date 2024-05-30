using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ServerInputBufferManager
{
    // Configuration
    [SerializeField] [Min(0)] private int minInputTickBufferSize = 2;
    [SerializeField] [Min(0)] private int maxInputTickBufferSize = 4;
    
    // Private variables
    private bool _fillingInputBuffer = true;
    private SortedList<int, InputPayload> _inputList = new();
    
    public List<InputPayload> DrawInputsToProcess()
    {
        List<InputPayload> inputPayloads = new();
        
        // Check whether we're good with our buffer
        CheckInputBufferFilling();
        if (!_fillingInputBuffer)
        {
            // We're above the minimum buffer size, so we can draw an input.
            // If the buffer is running over (maybe the Client has had a lag spike and his packets came all at once)
            // we draw multiple inputs until we're at maxsize.
            while (_inputList.Count > maxInputTickBufferSize)
            {
                inputPayloads.Add(DrawOldestInput());
            }
            
            // Draw the oldest Input
            inputPayloads.Add(DrawOldestInput());
        }
        else
        {
            // If we're filling our buffer we just return an empty input indicating with the client tick
            // that it didn't come from a client
            inputPayloads.Add(new InputPayload { Tick = -1 });
        }
        
        return inputPayloads;
    }
    
    public InputPayload DrawOldestInput()
    {
        var inputPayload = _inputList.Values[0];
        
        _inputList.RemoveAt(0);
        
        return inputPayload;
    }
    
    public void AddInputToList(InputPayload inputPayload)
    {
        _inputList[inputPayload.Tick] = inputPayload;
    }
    
    private void CheckInputBufferFilling()
    {
        // First determine if we're waiting for the buffer to fill up, or we can process inputs,
        // if we're at 0 we want to wait till the buffer is full and only then start spending inputs.
        // If we're above the buffer size start spending.
        if (_inputList.Count == 0)
        {
            _fillingInputBuffer = true;
        }
        
        if (_inputList.Count > minInputTickBufferSize)
        {
            _fillingInputBuffer = false;
        }
    }
}