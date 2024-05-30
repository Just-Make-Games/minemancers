using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerStateManager : StateManager
{
    public override EntityType EntityType => EntityType.Player;
    
    // Camera variables
    [SerializeField] private CinemachineVirtualCamera playerVirtualCamera;
    
    // Movement handler of our player
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform aimOrigin;
    
    // Helper classes
    [SerializeField] private ServerInputBufferManager serverInputBufferManager;
    private readonly PlayerInputManager _playerInputManager = new();
    
    // Private variables
    private StatePayload _latestPrivateState;
    private StatePayload _lastProcessedPrivateState;
    private int _lastProcessedInputClientTick;
    private bool _hadForcedPositionChangeThisTick;
    
    public override void Awake()
    {
        base.Awake();
        
        // Initialization of helper classes
        playerMovement.Initialize();
        _playerInputManager.Initialize(transform, aimOrigin);
        
        Interpolator = new PlayerMovementInterpolator();
        var playerMovementInterpolator = Interpolator as PlayerMovementInterpolator;
        
        playerMovementInterpolator?.Initialize(transform, aimOrigin, TimeBetweenTicks);
    }
    
    public override void Update()
    {
        base.Update();
        
        if (IsOwner)
        {
            // Update call on helper classes
            _playerInputManager.Update();
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            playerVirtualCamera.enabled = true;
            playerVirtualCamera.Priority = 1;
        }
        else
        {
            playerVirtualCamera.Priority = 0;
        }
    }
    
    public override void ApplyState(StatePayload state)
    {
        // (2.)
        if (IsOwner)
        {
            _latestPrivateState = state;
        }
        // (4.)
        else
        {
            Interpolator.SetNewState(state);
        }
    }
    
    public override void SetToState(StatePayload state)
    {
        playerMovement.SetPlayerToState(state, transform);
    }
    
    public override void PositionToTick(int tick)
    {
        transform.position = StateBuffer[BufferIndexForTick(tick)].Position;
    }
    
    public override void RepositionBack()
    {
        Interpolator.InterpolateLocal(0);
    }
    
    public void ForceSetPosition(Vector3 position)
    {
        var bufferIndex = NetworkManager.ServerTime.Tick % BufferSize;
        StateBuffer[bufferIndex].Position = position;
        StateBuffer[bufferIndex].HadForcedPositionChange = true;
        
        Interpolator.ForceUpdateState(StateBuffer[bufferIndex]);
        
        _hadForcedPositionChangeThisTick = true;
    }
    
    public override StatePayload Tick()
    {
        var localTick = NetworkManager.LocalTime.Tick;
        var serverTick = NetworkManager.ServerTime.Tick;
        
        // The 4 types of player objects with their corresponding behaviour are as follows:
        // (1.) owner and server - local client on server side (host): we read input, move by it, readout state, store state
        // (2.) owner and !server - local client on client side: we read input, store it in buffer, send the input to server, reconcile if needed, move by input, store the resulting state in buffer (for future reconcilliation)
        // (3.) !owner and server - remote client on server side: draw an input (or several) from our input buffer, process it, readout state
        // (4.) !owner and !server - remote client on client side: we set the received state to our interpolator
        
        // (1.) + (2.)
        if (IsOwner)
        {
            // Read input
            var inputThisTick = _playerInputManager.ReadoutInput(localTick);
            var bufferIndex = BufferIndexForTick(localTick);
            // Revert position to last state (since we have interpolated in the meantime)
            var bufferIndexForLastTick = BufferIndexForTick(localTick - 1);
            var lastTickBufferState = StateBuffer[bufferIndexForLastTick];
            
            if (!lastTickBufferState.Equals(default(StatePayload)))
                SetToState(lastTickBufferState);
            
            if (IsServer)
            {
                // (1.) Move by input
                playerMovement.MoveByInput(inputThisTick, TimeBetweenTicks);
            }
            else
            {
                // (2.) First store input and set it to server
                _playerInputManager.StoreInputInBuffer(inputThisTick);
                SendInputToServerRpc(inputThisTick);
                
                // Check for necessary reconciliation
                CheckForReconciliation();
                
                // Then locally predict our player state by our last input
                playerMovement.MoveByInput(inputThisTick, TimeBetweenTicks);
            }
            
            // Read out our resulting state
            var resultingState = ReadoutState(localTick, inputThisTick);
            // As a remote client we need to save this prediction into our buffer to check for reconciliation later,
            // as a server we need only it for the next tick
            StateBuffer[bufferIndex] = resultingState;
            
            if (IsServer)
            {
                // Mark a forced position change
                resultingState.HadForcedPositionChange = _hadForcedPositionChangeThisTick;
                _hadForcedPositionChangeThisTick = false;
            }
            
            // set the interpolator states
            Interpolator.SetNewState(resultingState);
            
            return resultingState;
        }
        
        //(3.)
        if (IsServer && !IsOwner)
        {
            // Reset position to the last state
            var bufferIndexForLastTick = BufferIndexForTick(localTick - 1);
            var lastTickBufferState = StateBuffer[bufferIndexForLastTick];
            
            if (!lastTickBufferState.Equals(default(StatePayload)))
                SetToState(lastTickBufferState);
            
            // Get a list of inputs through our serverInputBufferManager.
            // It usually returns a single input, in case of buffer overflow we get multiple inputs that we need to process.
            var inputsToProcess = serverInputBufferManager.DrawInputsToProcess();
            
            // Process the drawn inputs
            foreach (var inputToProcess in inputsToProcess)
            {
                ProcessInput(inputToProcess);
                
                if (inputToProcess.Tick != -1)
                    _lastProcessedInputClientTick = inputToProcess.Tick;
            }
            
            // Readout the resulting state
            var resultingState = ReadoutState(_lastProcessedInputClientTick, inputsToProcess[^1]);
            
            // If we have teleported in the last tick mark it so that remote players get teleported
            resultingState.HadForcedPositionChange = _hadForcedPositionChangeThisTick;
            _hadForcedPositionChangeThisTick = false;
            // Save the state in our buffer according to the server tick 
            var bufferIndexForProcessedTick = BufferIndexForTick(serverTick);
            StateBuffer[bufferIndexForProcessedTick] = resultingState;
            
            // Set the parameters for our local interpolator
            Interpolator.SetNewState(resultingState);
            
            return resultingState;
        }
        
        return default;
    }
    
    private void LateUpdate()
    {
        if (IsOwner)
        {
            // Update call on helper classes
            _playerInputManager.LateUpdate();
        }
    }
    
    private void CheckForReconciliation()
    {
        // If we have already processed the latest received state then return
        if (_lastProcessedPrivateState.Equals(_latestPrivateState))
            return;
        
        _lastProcessedPrivateState = _latestPrivateState;
        var bufferIndex = BufferIndexForTick(_lastProcessedPrivateState.Tick);
        // Check if the servers position and our position (at that tick) is different
        var positionError = Vector3.Distance(_lastProcessedPrivateState.Position, StateBuffer[bufferIndex].Position);
        
        if (!(positionError > 0.1f)) return;
        // If yes then reposition ourselves to the servers position and overwrite our state buffer
        StateBuffer[bufferIndex] = _lastProcessedPrivateState;
        
        SetToState(_lastProcessedPrivateState);
        Physics.SyncTransforms();
        
        // And then re-simulate all our movement by the inputs we stored in the meantime
        var tickToProcess = _lastProcessedPrivateState.Tick + 1;
        
        while (tickToProcess < NetworkManager.LocalTime.Tick)
        {
            var tickToProcessIndex = BufferIndexForTick(tickToProcess);
            
            // Read our stored input, simulate movement, store it, iterate
            var input = _playerInputManager.GetInputFromBuffer(tickToProcess);
            
            // Re-simulate movement
            playerMovement.MoveByInput(input, TimeBetweenTicks);
            
            // Store it
            var recalculatedState = ReadoutState(tickToProcess, input);
            StateBuffer[tickToProcessIndex] = recalculatedState;
            
            // Iterate
            tickToProcess++;
        }
    }
    
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendInputToServerRpc(InputPayload inputPayload)
    {
        // If the input that we received is older than the last processed tick means it arrived very late,
        // just discard it.
        if (inputPayload.Tick < _lastProcessedInputClientTick) 
            return;
        
        // Add the input to our list
        serverInputBufferManager.AddInputToList(inputPayload);
    }
    
    private void ProcessInput(InputPayload inputPayload)
    {
        transform.rotation = inputPayload.PlayerRotation;
        aimOrigin.localRotation = Quaternion.Euler(0, 0, inputPayload.AimRotationZ);
        playerMovement.MoveByInput(inputPayload, TimeBetweenTicks);
    }
    
    private StatePayload ReadoutState(int clientTick, InputPayload withInput)
    {
        return new StatePayload
        {
            Tick = clientTick,
            SnapshotTick = withInput.SnapshotTick,
            Position = transform.position,
            Rotation = transform.rotation,
            MovementInput = withInput.MovementInput,
            AimRotationZ = withInput.AimRotationZ,
            WeaponPrimaryDown = withInput.WeaponPrimaryButtonDown,
            WeaponSecondaryDown = withInput.WeaponSecondaryButtonDown,
        };
    }
}