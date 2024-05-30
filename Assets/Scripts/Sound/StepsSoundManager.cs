using UnityEngine;

public class StepsSoundManager : SoundManager
{
    [SerializeField] private PlayerAnimator animator;
    
    private void Awake()
    {
        animator.Step += PlayRandomSound;
    }
    
    private void OnDestroy()
    {
        animator.Step -= PlayRandomSound;
    }
}
