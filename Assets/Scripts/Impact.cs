using UnityEngine;

public class Impact : MonoBehaviour
{
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private float lifetime = 1f;
    
    private void Awake()
    {
        soundManager.PlayRandomSound();
        
        Destroy(gameObject, lifetime);
    }
}