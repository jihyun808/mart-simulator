using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마우스 설정 관리 (감도, Y축 반전)
/// PlayerPrefs로 저장/불러오기
/// Static 속성으로 게임 내에서 접근 가능
/// </summary>
public class ControlsSettings : MonoBehaviour
{
    [Header("Mouse Settings - Inspector에서 연결")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertYToggle;

    private const string PREF_MOUSE_SENSITIVITY = "MOUSE_SENSITIVITY";
    private const string PREF_INVERT_Y = "INVERT_Y";

    public static float MouseSensitivity { get; private set; } = 2f;
    public static bool InvertY { get; private set; } = false;

    private void Awake()
    {
        LoadSettings();
        InitMouseSensitivitySlider();
        InitInvertYToggle();
    }

    private void OnDestroy()
    {
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);

        if (invertYToggle != null)
            invertYToggle.onValueChanged.RemoveListener(OnInvertYChanged);
    }

    private void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat(PREF_MOUSE_SENSITIVITY, 2f);
        InvertY = PlayerPrefs.GetInt(PREF_INVERT_Y, 0) == 1;
    }

    private void InitMouseSensitivitySlider()
    {
        if (mouseSensitivitySlider == null) return;

        mouseSensitivitySlider.minValue = 0.1f;
        mouseSensitivitySlider.maxValue = 5f;
        mouseSensitivitySlider.value = MouseSensitivity;
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void InitInvertYToggle()
    {
        if (invertYToggle == null) return;

        invertYToggle.isOn = InvertY;
        invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
    }

    private void OnMouseSensitivityChanged(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat(PREF_MOUSE_SENSITIVITY, value);
        PlayerPrefs.Save();
    }

    private void OnInvertYChanged(bool invert)
    {
        InvertY = invert;
        PlayerPrefs.SetInt(PREF_INVERT_Y, invert ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = 2f;
        }

        if (invertYToggle != null)
        {
            invertYToggle.isOn = false;
        }

        PlayerPrefs.DeleteKey(PREF_MOUSE_SENSITIVITY);
        PlayerPrefs.DeleteKey(PREF_INVERT_Y);
        PlayerPrefs.Save();
    }
}