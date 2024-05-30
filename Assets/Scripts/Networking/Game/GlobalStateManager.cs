using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GlobalStateManager : NetworkBehaviour
{
    // Static int which tells us on what snapshot tick we are
    public static int CurrentSnapshotTick { get; private set; }
    
    // Static list of all state managers to synchronise.
    // We keep it static since these should be scene persistent
    // and could sometimes be called while no instance of this class is present (in scene transitions).
    private static readonly List<StateManager> StateManagers = new();
    
    public static void RegisterEntity(StateManager stateManager)
    {
        StateManagers.Add(stateManager);
    }
    
    public static void RemoveEntity(StateManager stateManager)
    {
        StateManagers.Remove(stateManager);
    }
    
    // Helper class to buffer received snapshots
    // (since the snapshots don't come in consistently by the nature of networked connections).
    [SerializeField] private SnapshotBufferManager snapshotBufferManager;
    
    // These methods are useful for lag compensation
    // in case the server wants to position all entities back to a time when the client has sent an input
    // (for example a ray cast)
    public void PositionEntitiesToSnapshot(int tick)
    {
        foreach (var stateManager in StateManagers)
        {
            stateManager.PositionToTick(tick);
        }
    }
    
    public void RepositionEntitiesBack()
    {
        foreach (var stateManager in StateManagers)
        {
            stateManager.RepositionBack();
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Ticks are based on the integrated NetworkTickSystem
        NetworkManager.NetworkTickSystem.Tick += Tick;
        
        // Set it to our beginning tick to not send an unnecessary InputPayload for tick 0 in the beginning
        CurrentSnapshotTick = NetworkManager.LocalTime.Tick;
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        NetworkManager.NetworkTickSystem.Tick -= Tick;
    }
    
    private void Tick()
    {
        var localTick = NetworkManager.LocalTime.Tick;
        
        // As a server we want to tell each entity to process a tick.
        // Then we create a snapshot, which holds information about the state of each networked entity and send it to all players.
        if (IsServer)
        {
            Dictionary<StateManager, StatePayload> snapshot = new();
            CurrentSnapshotTick = localTick;
            
            //tell each state manager to tick and then store the resulting state payload in a snapshot
            foreach (var stateManager in StateManagers)
            {
                var resultingState = stateManager.Tick();
                
                snapshot.Add(stateManager, resultingState);
            }
            
            // Send snapshot to all players
            SendSnapshotToClientsRpc(new StateSnapshot(localTick, snapshot));
        }
        
        // As a client we want to apply a new snapshot each tick
        else
        {
            // Check if there even is a new snapshot to process
            // (this might not be the case if we are experiencing lags or packet loss).
            // If there is no new snapshot the bool is false and the out StateSnapshot returns an empty (default) struct.
            var newSnapshot = snapshotBufferManager.NewSnapshot(out var snapshot);
            
            // Only update the tick if we have a new snapshot (indicating the StateSnapshot struct is not empty)
            // or else we get a tick of 0.
            if (newSnapshot)
                CurrentSnapshotTick = snapshot.Tick;
            
            //Tick each state manager and in case of a new snapshot apply it before that
            foreach (var stateManager in StateManagers)
            {
                if (newSnapshot)
                {
                    if (snapshot.Data.TryGetValue(stateManager, out var snapshotState))
                        stateManager.ApplyState(snapshotState);
                }
                
                stateManager.Tick();
            }
        }
    }
    
    [Rpc(SendTo.NotServer)]
    private void SendSnapshotToClientsRpc(StateSnapshot snapshot)
    {
        snapshotBufferManager.AddSnapshot(snapshot);
    }
    
    /// <summary>
    /// Performs a collision check for the rewind period of a projectile.
    /// </summary>
    /// <param name="inputPayloadSnapshotTick">The tick at which the projectile was instantiated locally by the client.</param>
    /// <param name="projectileOrigin">Origin position of projectile</param>
    /// <param name="travelVector">The Vector3 which defines the projectiles travelled path in the specified interval</param>
    /// <returns>Returns a Transform that would have been hit by a projectile in an interval. Null otherwise.</returns>
    public static Transform CheckForRewindPeriodHit(int inputPayloadSnapshotTick, Vector3 projectileOrigin,
        Vector3 travelVector)
    {
        // Iterate through entities
        foreach (var stateManager in StateManagers)
        {
            // Calculate move vector in interval period
            // Get the position of the entity at original shoot tick
            var stateAtShootTime = stateManager.RetrieveStatePayloadFromBuffer(inputPayloadSnapshotTick);
            
            if (Equals(stateAtShootTime, default))
            {
                // If it equals default then the state hasn't been initialized at that moment - e.g. shot happened before the entity got registered
                continue;
            }
            
            var intervalMoveVector = stateManager.transform.position - stateAtShootTime.Position;
            Debug.DrawLine(stateManager.transform.position, stateAtShootTime.Position, Color.red, 5f);
            
            // Calculate adjusted projectile trajectory
            var adjustedProjectileTrajectory = travelVector - intervalMoveVector;
            Debug.DrawLine(projectileOrigin, projectileOrigin + adjustedProjectileTrajectory, Color.green, 5f);
            
            // Reposition entity to snapshot tick
            stateManager.PositionToTick(inputPayloadSnapshotTick);
            Physics2D.SyncTransforms();
            
            // Create an ignore ray cast layer mask for the next overlap function and the ray cast function later
            var ignoreRaycastMask = ~LayerMask.GetMask("Ignore Raycast");
            // First check if the shoot origin is already in a collider, if it is then return that collider. method ended.
            var overlappingCollider = Physics2D.OverlapPoint(projectileOrigin, ignoreRaycastMask);
            
            if (overlappingCollider)
            {
                Debug.DrawLine(
                    overlappingCollider.transform.position + Vector3.left * (overlappingCollider.bounds.size.x * 0.5f),
                    overlappingCollider.transform.position + Vector3.right * (overlappingCollider.bounds.size.x * 0.5f),
                    Color.yellow, 5f);
                Debug.DrawLine(
                    overlappingCollider.transform.position + Vector3.up * (overlappingCollider.bounds.size.y * 0.5f),
                    overlappingCollider.transform.position + Vector3.down * (overlappingCollider.bounds.size.y * 0.5f),
                    Color.yellow, 5f);
                stateManager.RepositionBack();
                
                return overlappingCollider.transform;
            }
            
            // Set it into a simulation layer (can extract this method to the state manager later)
            var originalLayer = stateManager.gameObject.layer;
            var simulationLayer = LayerMask.NameToLayer("Simulation");
            var simulationMask = LayerMask.GetMask("Simulation");
            
            stateManager.gameObject.layer = simulationLayer;
            
            // Debugging purposes
            var collider = stateManager.GetComponent<Collider2D>();
            
            Debug.DrawLine(collider.transform.position + Vector3.left * (collider.bounds.size.x * 0.5f),
                collider.transform.position + Vector3.right * (collider.bounds.size.x * 0.5f), Color.yellow, 5f);
            Debug.DrawLine(collider.transform.position + Vector3.up * (collider.bounds.size.y * 0.5f),
                collider.transform.position + Vector3.down * (collider.bounds.size.y * 0.5f), Color.yellow, 5f);
            
            // Raycast possible hit in simulation layer and return to original layer
            var possibleHit = Physics2D.Raycast(projectileOrigin, adjustedProjectileTrajectory,
                adjustedProjectileTrajectory.magnitude, simulationMask);
            
            stateManager.gameObject.layer = originalLayer;
            
            // Reposition back
            stateManager.RepositionBack();
            
            // check for possible hit
            if (!possibleHit.collider) continue;
            
            Debug.DrawLine(possibleHit.point, possibleHit.point + Vector2.right * 1f, Color.cyan, 5f);
            Debug.Log($"Possible Hit {possibleHit.transform.name}");
            
            // If we recorded possible hit calculate at what percentage of the travel vector it got hit
            var percentageOfPathTravelled = possibleHit.distance / adjustedProjectileTrajectory.magnitude;
            // Then reposition this entity back to the tick of that time
            var snapshotTickAtPossibleHit = (int)Mathf.Lerp(inputPayloadSnapshotTick,
                NetworkManager.Singleton.ServerTime.Tick, percentageOfPathTravelled);
            
            stateManager.PositionToTick(snapshotTickAtPossibleHit);
            Physics2D.SyncTransforms();
            
            // Check if the hit actually occured in normal layer mask (ignoring the "Ignore Raycast" layer)
            var layerMask = ~ LayerMask.GetMask("Ignore Raycast");
            var actualHit = Physics2D.Raycast(projectileOrigin, travelVector, travelVector.magnitude,
                layerMask);
            
            // Reposition entity back to its original position
            stateManager.RepositionBack();
            
            // If it occured then return the transform
            if (!actualHit.collider) continue;
            
            Debug.DrawLine(actualHit.point, actualHit.point + Vector2.up * 1f, Color.black, 5f);
            
            // Continue entity iteration
            return actualHit.transform;
        }
        
        return null;
    }
}


public struct StateSnapshot : INetworkSerializable
{
    public int Tick;
    public Dictionary<StateManager, StatePayload> Data;
    
    public StateSnapshot(int tick, Dictionary<StateManager, StatePayload> data)
    {
        Tick = tick;
        Data = data;
    }
    
    // For each custom variable we want to serialize we need to implement a serialization and deserialization method
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            
            // Read the tick
            reader.ReadValueSafe(out Tick);
            
            Data = new Dictionary<StateManager, StatePayload>();
            
            reader.ReadValueSafe(out int dictionaryCount);
            
            for (var i = 0; i < dictionaryCount; i++)
            {
                // Get the entity type
                reader.ReadValueSafe(out ushort entityTypeUshort);
                
                // Read the state manager reference and find the corresponding StateManager
                reader.ReadValueSafe(out NetworkBehaviourReference networkBehaviourReference);
                
                StateManager stateManager = null;
                
                switch ((EntityType)entityTypeUshort)
                {
                    case EntityType.Player:
                        if (networkBehaviourReference.TryGet(out PlayerStateManager playerStateManager))
                        {
                            stateManager = playerStateManager;
                        }
                        else
                        {
                            Debug.Log("Failed to deserialize snapshot!");
                        }
                        
                        break;
                    case EntityType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                reader.ReadValueSafe(out StatePayload statePayload);
                
                // Don't add null key
                if (stateManager == null)
                    continue;
                
                Data.Add(stateManager, statePayload);
            }
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            
            // Write the tick
            writer.WriteValueSafe(Tick);
            
            // Serialize the dictionary count
            writer.WriteValueSafe(Data.Count);
            
            foreach (var pair in Data)
            {
                // What type of state manager is it?
                var stateManagerType = (ushort)pair.Key.EntityType;
                
                writer.WriteValueSafe(stateManagerType);
                // Add reference and the state payload itself
                writer.WriteValueSafe((NetworkBehaviourReference)pair.Key);
                writer.WriteValueSafe(pair.Value);
            }
        }
    }
}
