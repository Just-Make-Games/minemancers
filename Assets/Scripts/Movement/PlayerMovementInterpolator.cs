using UnityEngine;

public class PlayerMovementInterpolator : MovementInterpolator
{
    private Transform _aimOrigin;
    
    public void Initialize(Transform newTransform, Transform newAimOrigin, float newTimeBetweenTicks)
    {
        Initialize(newTransform, newTimeBetweenTicks);
        
        _aimOrigin = newAimOrigin;
    }
    
    public override StatePayload InterpolateLocal(float timeStep)
    {
        base.InterpolateLocal(timeStep);
        
        var lerpAmount = timeElapsed / timeBetweenTicks;
        var fromAimRotation = Quaternion.Euler(0, 0, from.AimRotationZ);
        var toAimRotation = Quaternion.Euler(0, 0, to.AimRotationZ);
        _aimOrigin.localRotation = Quaternion.Lerp(fromAimRotation, toAimRotation, lerpAmount);
        
        return to;
    }
    
    public override void InterpolateBetweenStates()
    {
        if (from.Equals(default(StatePayload)) || to.Equals(default(StatePayload)))
            return;
        
        base.InterpolateBetweenStates();
        
        var lerpAmount = timeElapsed / timeBetweenTicks;
        var fromAimRotation = Quaternion.Euler(0, 0, from.AimRotationZ);
        var toAimRotation = Quaternion.Euler(0, 0, to.AimRotationZ);
        _aimOrigin.localRotation = Quaternion.Lerp(fromAimRotation, toAimRotation, lerpAmount);
    }
}