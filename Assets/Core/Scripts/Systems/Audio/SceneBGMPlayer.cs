using UnityEngine;

public class SceneBGMPlayer : MonoBehaviour
{
    [Header("Audio BGM")]
    public AudioClip sceneBGM;

    void Start()
    {
        if (sceneBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(sceneBGM);
        }
        
    }
}
