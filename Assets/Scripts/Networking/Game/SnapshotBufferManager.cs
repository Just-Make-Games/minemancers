using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SnapshotBufferManager
{
    [SerializeField] [Min(0)] private int minBufferSize = 2;
    [SerializeField] [Min(0)] private int maxBufferSize = 4;

    private SortedList<int, StateSnapshot> _snapshots = new();
    private bool _fillingBuffer = true;

    public bool NewSnapshot(out StateSnapshot stateSnapshot)
    {
        stateSnapshot = new StateSnapshot();

        // Check whether we're good with our buffer
        CheckInputBufferFilling();
        
        //if we're filling our buffer return false
        if (_fillingBuffer) return false;
        
        // We're above the minimum buffer size.
        // If the buffer is running over we draw multiple until we're at maxsize.
        while (_snapshots.Count > maxBufferSize)
        {
            DrawOldestSnapshot();
        }
        
        // And then draw one more
        stateSnapshot = DrawOldestSnapshot();
        
        return true;
    }

    public StateSnapshot DrawOldestSnapshot()
    {
        var snapshot = _snapshots.Values[0];
        
        _snapshots.RemoveAt(0);
        
        return snapshot;
    }
    
    public void AddSnapshot(StateSnapshot stateSnapshot)
    {
        _snapshots[stateSnapshot.Tick] = stateSnapshot;
    }

    private void CheckInputBufferFilling()
    {
        // First determine if we're waiting for the buffer to fill up, or we can process, if we're at 0 we want to
        // wait till the buffer is full and only then start spending. If we're above the buffer size start spending.
        if (_snapshots.Count == 0)
        {
            _fillingBuffer = true;
        }
        if (_snapshots.Count > minBufferSize)
        {
            _fillingBuffer = false;
        }
    }
}
