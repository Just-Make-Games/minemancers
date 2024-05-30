using UnityEngine;

[System.Serializable]
public class MovementInterpolator
{
    protected Transform transform;
    protected float timeBetweenTicks;
    protected float timeElapsed;
    protected StatePayload from;
    protected StatePayload to;
    
    public void Initialize(Transform newTransform, float newTimeBetweenTicks)
    {
        transform = newTransform;
        timeBetweenTicks = newTimeBetweenTicks;
    }
    
    public virtual StatePayload InterpolateLocal(float timeStep)
    {
        timeElapsed += timeStep;
        
        var lerpAmount = timeElapsed / timeBetweenTicks;
        transform.rotation = Quaternion.Lerp(from.Rotation.normalized, to.Rotation.normalized, lerpAmount);
        transform.position = Vector3.Lerp(from.Position, to.Position, lerpAmount);
        
        return to;
    }
    
    public StatePayload InterpolateRemote(float timeStep)
    {
        timeElapsed += timeStep;
        
        InterpolateBetweenStates();
        
        return to;
    }
    
    public void SetNewState(StatePayload newTo)
    {
        timeElapsed = 0;
        from = to;
        to = newTo;
    }
    
    public void SetStates(StatePayload newFrom, StatePayload newTo)
    {
        timeElapsed = 0;
        from = newFrom;
        to = newTo;
    }
    
    public void ForceUpdateState(StatePayload state)
    {
        to = state;
    }
    
    public virtual void InterpolateBetweenStates()
    {
        // Don't interpolate if not fully initialized
        if (from.Equals(default(StatePayload)) || to.Equals(default(StatePayload)))
            return;
        
        var lerpAmount = timeElapsed / timeBetweenTicks;
        transform.rotation = Quaternion.Lerp(from.Rotation, to.Rotation, lerpAmount);
        // If we had a forced position change (teleport) then don't interpolate position
        transform.position = to.HadForcedPositionChange
            ? to.Position
            : Vector3.Lerp(from.Position, to.Position, lerpAmount);
    }
}