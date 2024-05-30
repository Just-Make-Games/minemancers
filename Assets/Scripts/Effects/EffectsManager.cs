using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EffectsManager : MonoBehaviour
{
    [SerializeField] private HealthPresenter healthPresenter;
    [SerializeField] private List<GameObject> damagePrefabs;
    [SerializeField] private float damagePrefabLifetimeSeconds = 5;
    
    private void Awake()
    {
        healthPresenter.OnHurt += OnHurt;
    }
    
    private void OnDestroy()
    {
        healthPresenter.OnHurt -= OnHurt;
    }
    
    private void OnHurt()
    {
        var damagePrefab = GetRandomPrefab();
        var damagePrefabInstance = Instantiate(damagePrefab, transform.position,
            Quaternion.Euler(0, 0, Random.Range(0, 360)));
        
        StartCoroutine(DestroyPrefabInstance(damagePrefabInstance));
    }
    
    private GameObject GetRandomPrefab()
    {
        var randomIndex = Random.Range(0, damagePrefabs.Count);
        
        return damagePrefabs[randomIndex];
    }
    
    private IEnumerator DestroyPrefabInstance(GameObject prefabInstance)
    {
        yield return new WaitForSeconds(damagePrefabLifetimeSeconds);
        
        Destroy(prefabInstance);
    }
}
