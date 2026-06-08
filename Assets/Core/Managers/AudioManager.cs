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
            LoadVolume();
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

    public void LoadVolume()
    {
        float savedBGM = PlayerPrefs.GetFloat("MasterBGM", 1f);
        float savedSFX = PlayerPrefs.GetFloat("MasterSFX", 1f);

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGM_Volume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFX_Volume", volume);
        PlayerPrefs.Save();
    }
}