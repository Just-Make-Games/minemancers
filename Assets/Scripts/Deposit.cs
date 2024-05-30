using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Deposit : NetworkBehaviour, IDamageable
{
    [SerializeField] private Transform ammoSpawnAreaCentre;
    [SerializeField] private float ammoSpawnAreaRadius = 0.5f;
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private int minAmmoAmount = 1;
    [SerializeField] private int maxAmmoAmount = 1;
    [SerializeField] private int maxAmmoSpawns = 3;
    [SerializeField] private float rechargeDuration = 5;
    
    private int _currentAmmoSpawns;
    
    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        
        Invoke(nameof(Spawn), rechargeDuration);
        
        base.OnNetworkDespawn();
    }
    
    public void TakeDamage(float amount, Player source)
    {
        _currentAmmoSpawns++;
            
        var ammoAmount = Random.Range(minAmmoAmount, maxAmmoAmount + 1);
        
        for (var i = 0; i < ammoAmount; i++)
        {
            SpawnAmmo();
        }
        
        if (_currentAmmoSpawns < maxAmmoSpawns) return;
        
        Despawn();
    }
    
    private Vector2 GetRandomSpawnPoint()
    {
        return (Vector2) ammoSpawnAreaCentre.position + Random.insideUnitCircle * ammoSpawnAreaRadius;
    }
    
    private void SpawnAmmo()
    {
        if (!IsServer)
        {
            return;
        }
        
        var spawnPoint = GetRandomSpawnPoint();
        var instance = Instantiate(ammoPrefab, spawnPoint, Quaternion.Euler(0, 0,
            Random.Range(0, 360)));
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        
        instanceNetworkObject.Spawn();
    }
    
    private void Spawn()
    {
        if (!IsServer || IsSpawned)
        {
            return;
        }
        
        gameObject.SetActive(true);
        
        _currentAmmoSpawns = 0;
        
        NetworkObject.Spawn();
    }
    
    private void Despawn()
    {
        if (!IsServer)
        {
            return;
        }
        
        NetworkObject.Despawn(false);
    }
}
