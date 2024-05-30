using Unity.Netcode;
using UnityEngine;

public struct StatePayload : INetworkSerializable
{
    // State info
    public int Tick;
    public int SnapshotTick;
    
    // Position state
    public Vector3 Position;
    public Quaternion Rotation;
    public bool HadForcedPositionChange;
    
    // Input
    public Vector2 MovementInput;
    public float AimRotationZ;
    public bool WeaponPrimaryDown;
    public bool WeaponSecondaryDown;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Tick);
        serializer.SerializeValue(ref SnapshotTick);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref MovementInput);
        serializer.SerializeValue(ref AimRotationZ);
        serializer.SerializeValue(ref Rotation);
        serializer.SerializeValue(ref HadForcedPositionChange);
        serializer.SerializeValue(ref WeaponPrimaryDown);
        serializer.SerializeValue(ref WeaponSecondaryDown);
    }
}


public enum EntityType : ushort
{
    None = 0,
    Player,
    //Critter, (this will be useful once we start using NPCs since they have different state calculation and behaviours)
}


public abstract class StateManager : NetworkBehaviour
{
    public static int BufferIndexForTick(int tick) => tick % BufferSize;
    
    // How many ticks of states do we store?
    public const ushort BufferSize = 1024;
    
    // Event to call update with current state
    public delegate void UpdateWithState(StatePayload state);
    public event UpdateWithState OnUpdateWithState;
    
    public virtual EntityType EntityType => EntityType.None;
    
    // Helper methods for variables
    protected float TimeBetweenTicks => 1f / NetworkManager.NetworkTickSystem.TickRate;
    // Interpolator helper class
    protected MovementInterpolator Interpolator = new();
    // State buffer for keeping history of our last [buffer_size] amount of states
    protected StatePayload[] StateBuffer;
    
    // Current State for calling on handlers and repositioning
    private StatePayload _currentState;
    
    public abstract StatePayload Tick();
    
    public abstract void ApplyState(StatePayload state);
    
    public abstract void SetToState(StatePayload state);
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GlobalStateManager.RegisterEntity(this);
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        GlobalStateManager.RemoveEntity(this);
    }
    
    public virtual void Awake()
    {
        Interpolator.Initialize(transform, TimeBetweenTicks);
        
        // Initialization of state buffer by protected constant
        StateBuffer = new StatePayload[BufferSize];
    }
    
    public virtual void Update()
    {
        // Interpolate between states, get the currentState for handlers
        if (IsOwner || IsServer)
        {
            _currentState = Interpolator.InterpolateLocal(Time.deltaTime);
        }
        else
        {
            _currentState = Interpolator.InterpolateRemote(Time.deltaTime);
        }
        
        // Send state to subscribers
        OnUpdateWithState?.Invoke(_currentState);
    }
    
    public virtual void PositionToTick(int tick)
    {
    }
    
    public virtual void RepositionBack()
    {
    }
    
    public StatePayload RetrieveStatePayloadFromBuffer(int tick)
    {
        return StateBuffer[BufferIndexForTick(tick)];
    }
}