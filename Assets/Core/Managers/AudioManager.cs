using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip, bool stopPrevious = false)
    {
        if (clip != null && sfxSource != null)
        {
            if (stopPrevious)
            {
                sfxSource.Stop();
            }

            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip != null && bgmSource != null)
        {
            if (bgmSource.clip == clip) return;

            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }
}
