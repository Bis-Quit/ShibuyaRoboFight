using UnityEngine;
using UnityEngine.UI;

public class SettingsUIManager : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void OnEnable()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("MasterBGM", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("MasterSFX", 1f);

        bgmSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
    }

    private void OnBGMChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterBGM", value);
        PlayerPrefs.Save();
    }

    private void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterSFX", value);
        PlayerPrefs.Save();
    }
}