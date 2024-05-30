using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected List<AudioClip> clips;
    [SerializeField][Range(0, 1)] protected float minVolume = 1f;
    [SerializeField][Range(0, 1)] protected float maxVolume = 1f;
    [SerializeField][Range(-3, 3)] protected float minPitch = 1f;
    [SerializeField][Range(-3, 3)] protected float maxPitch = 1f;
    
    public void PlayRandomSound()
    {
        var soundsN = clips.Count;
        
        if (soundsN <= 0)
        {
            return;
        }
        
        var randomSoundIndex = Random.Range(0, soundsN);
        var clip = clips[randomSoundIndex];
        
        PlaySound(clip);
    }
    
    protected void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.volume = Random.Range(minVolume, maxVolume);
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        
        audioSource.Play();
    }
}
