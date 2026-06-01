using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [Header("Audio SFX")]
    public AudioClip clickSFX;

    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(PlayClickSound); 
    }

    private void PlayClickSound()
    {
        if (clickSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSFX);
        }
    }
}
