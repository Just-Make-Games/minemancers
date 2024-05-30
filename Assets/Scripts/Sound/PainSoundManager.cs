using UnityEngine;

public class PainSoundManager : SoundManager
{
    [SerializeField] private HealthPresenter healthPresenter;
    
    private void Awake()
    {
        healthPresenter.OnHurt += PlayRandomSound;
    }
    
    private void OnDestroy()
    {
        healthPresenter.OnHurt -= PlayRandomSound;
    }
}
