using UnityEngine;

public class DeathSoundManager : SoundManager
{
    [SerializeField] private HealthPresenter healthPresenter;
    
    private void Awake()
    {
        healthPresenter.OnDeath += OnDeath;
    }
    
    private void OnDestroy()
    {
        healthPresenter.OnDeath -= OnDeath;
    }
    
    private void OnDeath(Player player)
    {
        PlayRandomSound();
    }
}
