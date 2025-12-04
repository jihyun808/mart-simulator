// AudioSettings.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    private const string MASTER_PARAM = "MasterVol";
    private const string BGM_PARAM = "BGMVol";
    private const string SFX_PARAM = "SFXVol";
    private const string UI_PARAM = "UIVol";

    private const string MASTER_KEY = "MASTER_VOL";
    private const string BGM_KEY = "BGM_VOL";
    private const string SFX_KEY = "SFX_VOL";
    private const string UI_KEY = "UI_VOL";

    private void Awake()
    {
        InitSlider(masterSlider, MASTER_KEY, 1f, OnMasterVolumeChanged);
        InitSlider(bgmSlider, BGM_KEY, 1f, OnBGMVolumeChanged);
        InitSlider(sfxSlider, SFX_KEY, 1f, OnSFXVolumeChanged);
        InitSlider(uiSlider, UI_KEY, 1f, OnUIVolumeChanged);
    }

    private void InitSlider(Slider slider, string prefsKey, float defaultValue, UnityEngine.Events.UnityAction<float> onChanged)
    {
        if (slider == null) return;

        float value = PlayerPrefs.GetFloat(prefsKey, defaultValue);
        slider.minValue = 0.0001f;
        slider.maxValue = 1f;
        slider.value = value;

        slider.onValueChanged.AddListener(onChanged);
        onChanged.Invoke(value);
    }

    private void SetVolume(string parameter, float sliderValue)
    {
        if (masterMixer == null) return;

        float dB = Mathf.Log10(sliderValue) * 20f;
        masterMixer.SetFloat(parameter, dB);
    }

    public void OnMasterVolumeChanged(float value)
    {
        SetVolume(MASTER_PARAM, value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void OnBGMVolumeChanged(float value)
    {
        SetVolume(BGM_PARAM, value);
        PlayerPrefs.SetFloat(BGM_KEY, value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SetVolume(SFX_PARAM, value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    public void OnUIVolumeChanged(float value)
    {
        SetVolume(UI_PARAM, value);
        PlayerPrefs.SetFloat(UI_KEY, value);
    }

    public void ResetToDefaults()
    {
        if (masterSlider != null) masterSlider.value = 1f;
        if (bgmSlider != null) bgmSlider.value = 1f;
        if (sfxSlider != null) sfxSlider.value = 1f;
        if (uiSlider != null) uiSlider.value = 1f;

        PlayerPrefs.DeleteKey(MASTER_KEY);
        PlayerPrefs.DeleteKey(BGM_KEY);
        PlayerPrefs.DeleteKey(SFX_KEY);
        PlayerPrefs.DeleteKey(UI_KEY);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}