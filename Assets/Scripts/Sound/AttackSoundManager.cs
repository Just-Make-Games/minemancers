using UnityEngine;

public class AttackSoundManager : SoundManager
{
    [SerializeField] private PlayerAnimator animator;
    
    private void Awake()
    {
        animator.Attack += PlayRandomSound;
    }
    
    private void OnDestroy()
    {
        animator.Attack -= PlayRandomSound;
    }
}
