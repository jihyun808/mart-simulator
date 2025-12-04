// VideoSettings.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettings : MonoBehaviour
{
    [Header("Resolution & Fullscreen")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Quality")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("VSync")]
    [SerializeField] private Toggle vsyncToggle;

    [Header("Brightness")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Image brightnessOverlay;

    private readonly int[] widths = { 1920, 1600, 1366, 1280 };
    private readonly int[] heights = { 1080, 900, 768, 720 };

    private const string PREF_RESOLUTION_INDEX = "RESOLUTION_INDEX";
    private const string PREF_FULLSCREEN = "FULLSCREEN";
    private const string PREF_QUALITY = "QUALITY";
    private const string PREF_VSYNC = "VSYNC";
    private const string PREF_BRIGHTNESS = "BRIGHTNESS";

    private void Start()
    {
        InitResolutionDropdown();
        InitFullscreenToggle();
        InitQualityDropdown();
        InitVSyncToggle();
        InitBrightness();
    }

    private void OnDestroy()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(SetResolution);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(SetQuality);

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.RemoveListener(SetVSync);

        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
    }

    private void InitResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        int savedIndex = PlayerPrefs.GetInt(PREF_RESOLUTION_INDEX, -1);
        int currentIndex = 0;

        for (int i = 0; i < widths.Length; i++)
        {
            string label = $"{widths[i]} × {heights[i]}";
            options.Add(label);

            if (savedIndex == -1 && Screen.width == widths[i] && Screen.height == heights[i])
            {
                currentIndex = i;
            }
        }

        if (savedIndex >= 0 && savedIndex < widths.Length)
        {
            currentIndex = savedIndex;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void InitFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        bool saved = PlayerPrefs.GetInt(PREF_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.isOn = saved;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void InitQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();

        var options = new List<string> { "Low", "Medium", "High" };
        qualityDropdown.AddOptions(options);

        int saved = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());
        int current = Mathf.Clamp(saved, 0, 2);
        qualityDropdown.value = current;
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    private void InitVSyncToggle()
    {
        if (vsyncToggle == null) return;

        bool saved = PlayerPrefs.GetInt(PREF_VSYNC, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        vsyncToggle.isOn = saved;
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    private void InitBrightness()
    {
        if (brightnessSlider == null) return;

        brightnessSlider.minValue = 0f;
        brightnessSlider.maxValue = 1f;

        float saved = PlayerPrefs.GetFloat(PREF_BRIGHTNESS, 1f);
        brightnessSlider.value = saved;
        brightnessSlider.onValueChanged.AddListener(SetBrightness);

        SetBrightness(saved);
    }

    private void SetResolution(int index)
    {
        int w = widths[index];
        int h = heights[index];

        Screen.SetResolution(w, h, Screen.fullScreen);
        PlayerPrefs.SetInt(PREF_RESOLUTION_INDEX, index);
        PlayerPrefs.Save();
    }

    private void SetFullscreen(bool isFull)
    {
        int index = resolutionDropdown != null ? resolutionDropdown.value : 0;
        int w = widths[index];
        int h = heights[index];

        Screen.SetResolution(w, h, isFull);
        PlayerPrefs.SetInt(PREF_FULLSCREEN, isFull ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetQuality(int index)
    {
        int clamped = Mathf.Clamp(index, 0, 2);
        QualitySettings.SetQualityLevel(clamped, true);
        PlayerPrefs.SetInt(PREF_QUALITY, clamped);
        PlayerPrefs.Save();
    }

    private void SetVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        PlayerPrefs.SetInt(PREF_VSYNC, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            float minAlpha = 0.0f;
            float maxAlpha = 0.8f;
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, Mathf.Clamp01(value));

            Color c = brightnessOverlay.color;
            c.a = alpha;
            brightnessOverlay.color = c;
        }

        PlayerPrefs.SetFloat(PREF_BRIGHTNESS, value);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        if (resolutionDropdown != null)
        {
            int defaultIndex = 0;
            resolutionDropdown.value = defaultIndex;
            SetResolution(defaultIndex);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = true;
            SetFullscreen(true);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = 2;
            SetQuality(2);
        }

        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = true;
            SetVSync(true);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.value = 1f;
            SetBrightness(1f);
        }

        PlayerPrefs.DeleteKey(PREF_RESOLUTION_INDEX);
        PlayerPrefs.DeleteKey(PREF_FULLSCREEN);
        PlayerPrefs.DeleteKey(PREF_QUALITY);
        PlayerPrefs.DeleteKey(PREF_VSYNC);
        PlayerPrefs.DeleteKey(PREF_BRIGHTNESS);
        PlayerPrefs.Save();
    }
}