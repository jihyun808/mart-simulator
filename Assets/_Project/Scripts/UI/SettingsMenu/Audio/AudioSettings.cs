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

    // AudioMixer 파라미터 이름 (믹서에서 노출한 이름과 똑같이 적기)
    private const string MASTER_PARAM = "MasterVol";
    private const string BGM_PARAM    = "BGMVol";
    private const string SFX_PARAM    = "SFXVol";
    private const string UI_PARAM     = "UIVol";

    // PlayerPrefs 키
    private const string MASTER_KEY = "MASTER_VOL";
    private const string BGM_KEY    = "BGM_VOL";
    private const string SFX_KEY    = "SFX_VOL";
    private const string UI_KEY     = "UI_VOL";

    private void Awake()
    {
        // 슬라이더 기본 범위 (0~1)로 가정
        InitSlider(masterSlider, MASTER_KEY, 1f, OnMasterVolumeChanged);
        InitSlider(bgmSlider,    BGM_KEY,    1f, OnBGMVolumeChanged);
        InitSlider(sfxSlider,    SFX_KEY,    1f, OnSFXVolumeChanged);
        InitSlider(uiSlider,     UI_KEY,     1f, OnUIVolumeChanged);
    }

    private void InitSlider(Slider slider, string prefsKey, float defaultValue, UnityEngine.Events.UnityAction<float> onChanged)
    {
        if (slider == null) return;

        float value = PlayerPrefs.GetFloat(prefsKey, defaultValue);
        slider.minValue = 0.0001f;  // dB 변환할 때 -∞ 방지용
        slider.maxValue = 1f;
        slider.value = value;

        slider.onValueChanged.AddListener(onChanged);

        // 처음에도 한 번 적용
        onChanged.Invoke(value);
    }

    // 0~1 값을 dB(-80 ~ 0)로 변환해서 Mixer에 넣기
    private void SetVolume(string parameter, float sliderValue)
    {
        if (masterMixer == null) return;

        float dB = Mathf.Log10(sliderValue) * 20f; // 0.0001~1 → -80~0 정도
        masterMixer.SetFloat(parameter, dB);
    }

    // ---- 콜백 ----
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

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
