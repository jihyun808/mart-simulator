using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // Toggle, Slider
using TMPro;           // TMP_Dropdown

public class VideoSettings : MonoBehaviour
{
    [Header("Resolution & Fullscreen")]
    [SerializeField] private TMP_Dropdown resolutionDropdown; // 해상도
    [SerializeField] private Toggle fullscreenToggle;         // 전체화면

    [Header("Quality")]
    [SerializeField] private TMP_Dropdown qualityDropdown;    // 그래픽 품질 (낮음/보통/높음)

    [Header("VSync")]
    [SerializeField] private Toggle vsyncToggle;              // V-Sync

    [Header("Brightness")]
    [SerializeField] private Slider brightnessSlider;         // 밝기 슬라이더 (0~1)
    [SerializeField] private Image brightnessOverlay;         // 화면 전체 덮는 검은 이미지

    // 사용할 해상도 목록 (드롭다운 순서랑 같음)
    private readonly int[] widths  = { 1920, 1600, 1366, 1280 };
    private readonly int[] heights = { 1080,  900,  768,  720 };

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
        // 리스너 정리
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

    #region Resolution & Fullscreen

    private void InitResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < widths.Length; i++)
        {
            string label = $"{widths[i]} × {heights[i]}";
            options.Add(label);

            if (Screen.width == widths[i] && Screen.height == heights[i])
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void InitFullscreenToggle()
    {
        if (fullscreenToggle == null) return;

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void SetResolution(int index)
    {
        int w = widths[index];
        int h = heights[index];

        Screen.SetResolution(w, h, Screen.fullScreen);
        Debug.Log($"[VideoSettings] Resolution: {w} x {h}, fullscreen={Screen.fullScreen}");
    }

    private void SetFullscreen(bool isFull)
    {
        // 현재 선택된 해상도 유지한 채 전체화면 On/Off
        int index = resolutionDropdown != null ? resolutionDropdown.value : 0;

        int w = widths[index];
        int h = heights[index];

        Screen.SetResolution(w, h, isFull);
        Debug.Log($"[VideoSettings] Fullscreen: {isFull}, {w} x {h}");
    }

    #endregion

    #region Quality

    private void InitQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();

        // 드롭다운에 보일 텍스트 (낮음/보통/높음)
        var options = new List<string> { "낮음", "보통", "높음" };
        qualityDropdown.AddOptions(options);

        // 현재 QualityLevel을 0~2 사이로 클램프해서 반영
        int current = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, 2);
        qualityDropdown.value = current;
        qualityDropdown.RefreshShownValue();

        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    private void SetQuality(int index)
    {
        // Project Settings > Quality 에서
        // 최소 3개 레벨을 만들어두고 (0,1,2) 낮음/보통/높음 순으로 맞춰 두면 됨.
        int clamped = Mathf.Clamp(index, 0, 2);
        QualitySettings.SetQualityLevel(clamped, true);
        Debug.Log($"[VideoSettings] Quality Level: {clamped} ({QualitySettings.names[clamped]})");
    }

    #endregion

    #region VSync

    private void InitVSyncToggle()
    {
        if (vsyncToggle == null) return;

        vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    private void SetVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        Debug.Log($"[VideoSettings] VSync: {(isOn ? "On" : "Off")}");
    }

    #endregion

    #region Brightness

    private void InitBrightness()
    {
        if (brightnessSlider == null) return;

        // 슬라이더 값 0~1 가정
        // 기본값 1 (가장 밝게)
        if (brightnessSlider.minValue != 0f || brightnessSlider.maxValue != 1f)
        {
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 1f;
        }

        if (brightnessSlider.value <= 0f)
            brightnessSlider.value = 1f;

        brightnessSlider.onValueChanged.AddListener(SetBrightness);

        // 시작할 때 한 번 적용
        SetBrightness(brightnessSlider.value);
    }

    private void SetBrightness(float value)
    {
    if (brightnessOverlay != null)
    {
        float minAlpha = 0.0f;  // 가장 밝을 때
        float maxAlpha = 0.8f;  // 가장 어두울 때 (완전 검정 X)

        // value 1 → alpha 0, value 0 → alpha 0.4
        float alpha = Mathf.Lerp(maxAlpha, minAlpha, Mathf.Clamp01(value));

        Color c = brightnessOverlay.color;
        c.a = alpha;
        brightnessOverlay.color = c;
    }

    Debug.Log($"[VideoSettings] Brightness: {value}");
    }

    #endregion
}
