using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public struct InputPayload : INetworkSerializable
{
    // Payload info
    public int Tick;
    public int SnapshotTick;
    
    // Player movement
    public Vector2 MovementInput;
    public Quaternion PlayerRotation;
    
    // Player inputs
    public float AimRotationZ;
    public bool WeaponPrimaryButtonDown;
    public bool WeaponSecondaryButtonDown;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref SnapshotTick);
        serializer.SerializeValue(ref MovementInput);
        serializer.SerializeValue(ref PlayerRotation);
        serializer.SerializeValue(ref AimRotationZ);
        serializer.SerializeValue(ref WeaponPrimaryButtonDown);
        serializer.SerializeValue(ref WeaponSecondaryButtonDown);
    }
}

[System.Serializable]
public class PlayerInputManager
{
    // References
    private Transform _transform;
    private Transform _aimOrigin;
    private Camera _cam;
    
    // Buffer
    private InputPayload[] _inputBuffer;
    
    // Bool to catch input in update
    private bool _weaponPrimaryDownPressedThisTick;
    private bool _weaponSecondaryDownPressedThisTick;
    
    public void Initialize(Transform transform, Transform aimOrigin)
    {
        _transform = transform;
        _aimOrigin = aimOrigin;
        _inputBuffer = new InputPayload[StateManager.BufferSize];
    }
    
    public void Update()
    {
        // Catch the inputs to not miss them
        if (InputManager.Singleton.GetWeaponPrimaryDown())
            _weaponPrimaryDownPressedThisTick = true;
        
        if (InputManager.Singleton.GetWeaponSecondaryDown())
            _weaponSecondaryDownPressedThisTick = true;
    }
    
    public void LateUpdate()
    {
        Aim();
    }
    
    public void StoreInputInBuffer(InputPayload inputPayload)
    {
        _inputBuffer[StateManager.BufferIndexForTick(inputPayload.Tick)] = inputPayload;
    }
    
    public InputPayload GetInputFromBuffer(int tick)
    {
        return _inputBuffer[StateManager.BufferIndexForTick(tick)];
    }
    
    public InputPayload ReadoutInput(int clientTick)
    {
        // Readout input
        InputPayload input = new()
        {
            Tick = clientTick,
            SnapshotTick = GlobalStateManager.CurrentSnapshotTick,
            MovementInput = InputManager.Singleton.GetPlayerMovement(),
            PlayerRotation = _transform.rotation,
            
            AimRotationZ = _aimOrigin.localRotation.eulerAngles.z,
            WeaponPrimaryButtonDown = _weaponPrimaryDownPressedThisTick,
            WeaponSecondaryButtonDown = _weaponSecondaryDownPressedThisTick,
        };
        
        // Reset frame trackers
        _weaponPrimaryDownPressedThisTick = false;
        _weaponSecondaryDownPressedThisTick = false;
        
        return input;
    }
    
    private void Aim()
    {
        SetCamera();
        
        Vector2 mousePositionToWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var direction = mousePositionToWorld - (Vector2)_transform.position;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var rotation = Quaternion.Euler(0, 0, angle);
        _aimOrigin.rotation = rotation;
    }
    
    /// <summary>
    /// Requires the reference to the main camera in the scene if it was lost.
    /// We should probably refactor the approach for this, as this can potentially be expensive,
    /// if the camera cannot be found immediately.
    /// </summary>
    private void SetCamera()
    {
        if (_cam) return;
        
        _cam = Camera.main;
    }
}