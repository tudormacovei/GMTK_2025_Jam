using System.Runtime.CompilerServices;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlaySfx(AudioClip audioClip)
    {
        if (audioClip == null) return;
        sfxSource.PlayOneShot(audioClip, sfxVolume);
    }

    public void PlaySfxInterrupt(AudioClip clip)
    {
        if (clip == null) return;

        // Stop whatever is playing, then play the new clip
        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.volume = sfxVolume;
        sfxSource.Play();
    }

    public bool IsSFXPlaying()
    {
        return sfxSource.isPlaying;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
