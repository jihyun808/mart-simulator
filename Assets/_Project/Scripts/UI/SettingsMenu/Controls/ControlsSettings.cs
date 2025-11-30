using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#region 입력 액션 / 설정 저장

public enum Action
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Jump,
    Run,
    Crouch,
    Interact,
    Inventory1,
    Inventory2,
    Inventory3,
    Inventory4,
    ListToggle,
    Pause,
    Throw,  // 좌클릭
    Grab    // 우클릭
}

public static class InputSettings
{
    public static Dictionary<Action, KeyCode> Keys = new Dictionary<Action, KeyCode>()
    {
        { Action.MoveForward, KeyCode.W },
        { Action.MoveBackward, KeyCode.S },
        { Action.MoveLeft, KeyCode.A },
        { Action.MoveRight, KeyCode.D },
        { Action.Jump, KeyCode.Space },
        { Action.Run, KeyCode.LeftShift },
        { Action.Crouch, KeyCode.LeftControl },
        { Action.Interact, KeyCode.E },
        { Action.Inventory1, KeyCode.F1 },
        { Action.Inventory2, KeyCode.F2 },
        { Action.Inventory3, KeyCode.F3 },
        { Action.Inventory4, KeyCode.F4 },
        { Action.ListToggle, KeyCode.Tab },
        { Action.Pause, KeyCode.Escape },
        { Action.Throw, KeyCode.Mouse0 }, // 좌클릭
        { Action.Grab, KeyCode.Mouse1 },  // 우클릭
    };

    public static float MouseSensitivity = 1.0f;
    public static bool InvertY = false;

    private const string PREF_MOUSE_SENS = "MouseSensitivity";
    private const string PREF_INVERT_Y   = "InvertY";
    private const string PREF_KEY_PREFIX = "KEY_";

    /// <summary>PlayerPrefs 에서 설정 불러오기</summary>
    public static void Load()
    {
        MouseSensitivity = PlayerPrefs.GetFloat(PREF_MOUSE_SENS, 1.0f);
        InvertY = PlayerPrefs.GetInt(PREF_INVERT_Y, 0) == 1;

        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            string key = PREF_KEY_PREFIX + action.ToString();
            if (PlayerPrefs.HasKey(key))
            {
                KeyCode code = (KeyCode)PlayerPrefs.GetInt(key);
                Keys[action] = code;
            }
        }
    }

    public static void SaveMouseSensitivity(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat(PREF_MOUSE_SENS, value);
    }

    public static void SaveInvertY(bool invert)
    {
        InvertY = invert;
        PlayerPrefs.SetInt(PREF_INVERT_Y, invert ? 1 : 0);
    }

    public static void SaveKey(Action action, KeyCode code)
    {
        Keys[action] = code;
        string key = PREF_KEY_PREFIX + action.ToString();
        PlayerPrefs.SetInt(key, (int)code);
    }
}

#endregion

#region UI 바인딩용 클래스

[Serializable]
public class KeyBindingWidget
{
    public Action action;      // 어떤 기능인지
    public Button button;      // 키 바꾸기 버튼
    public TMP_Text label;     // 버튼에 표시할 텍스트
}

#endregion

public class ControlsSettings : MonoBehaviour
{
    [Header("마우스 설정")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertYToggle;

    [Header("키 바인딩들")]
    [SerializeField] private List<KeyBindingWidget> keyBindingWidgets;

    private bool isRebinding = false;
    private Action currentAction;

    private void Awake()
    {
        // 저장된 설정 불러오기
        InputSettings.Load();

        // 슬라이더 / 토글 값 초기화
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = 0.1f;
            mouseSensitivitySlider.maxValue = 3f;
            mouseSensitivitySlider.value = InputSettings.MouseSensitivity;
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (invertYToggle != null)
        {
            invertYToggle.isOn = InputSettings.InvertY;
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
        }

        // 키 바인딩 UI 세팅
        foreach (var widget in keyBindingWidgets)
        {
            UpdateKeyLabel(widget);

            // 버튼 눌렀을 때 재바인딩 시작
            var capturedAction = widget.action;
            widget.button.onClick.AddListener(() => BeginRebind(capturedAction));
        }
    }

    private void Update()
    {
        if (!isRebinding) return;

        // 아무 키나 눌렸는지 체크
        if (Input.anyKeyDown)
        {
            KeyCode pressedKey = DetectPressedKey();
            if (pressedKey != KeyCode.None)
            {
                ApplyRebind(pressedKey);
            }
        }
    }

    #region 마우스 감도 / Y축 반전

    private void OnMouseSensitivityChanged(float value)
    {
        InputSettings.SaveMouseSensitivity(value);
        // 실제 카메라 컨트롤 스크립트에서 InputSettings.MouseSensitivity 를 읽어가면 됨
    }

    private void OnInvertYChanged(bool invert)
    {
        InputSettings.SaveInvertY(invert);
        // 마찬가지로 카메라 스크립트에서 InputSettings.InvertY 사용
    }

    #endregion

    #region 키 바인딩 관련

    private void BeginRebind(Action action)
    {
        if (isRebinding) return;

        isRebinding = true;
        currentAction = action;

        // 해당 액션 버튼 텍스트를 "Press key..." 처럼 바꿔주기
        var widget = keyBindingWidgets.Find(w => w.action == action);
        if (widget != null && widget.label != null)
        {
            widget.label.text = "입력 대기...";
        }

        Debug.Log($"Rebind 시작: {action}");
    }

    private KeyCode DetectPressedKey()
    {
        // 모든 KeyCode 순회해서 어떤 키가 눌렸는지 찾기
        foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(code))
            {
                return code;
            }
        }
        return KeyCode.None;
    }

    private void ApplyRebind(KeyCode newKey)
    {
        isRebinding = false;

        // 저장
        InputSettings.SaveKey(currentAction, newKey);

        // UI 텍스트 갱신
        var widget = keyBindingWidgets.Find(w => w.action == currentAction);
        if (widget != null)
        {
            UpdateKeyLabel(widget);
        }

        Debug.Log($"Rebind 완료: {currentAction} -> {newKey}");
    }

    private void UpdateKeyLabel(KeyBindingWidget widget)
    {
        if (widget.label == null) return;

        KeyCode key = InputSettings.Keys[widget.action];
        widget.label.text = KeyCodeToString(key);
    }

    private string KeyCodeToString(KeyCode code)
    {
        switch (code)
        {
            case KeyCode.LeftShift:   return "Left Shift";
            case KeyCode.RightShift:  return "Right Shift";
            case KeyCode.LeftControl: return "Left Ctrl";
            case KeyCode.RightControl:return "Right Ctrl";
            case KeyCode.Space:       return "Space";
            case KeyCode.Mouse0:      return "Left Click";
            case KeyCode.Mouse1:      return "Right Click";
            default:                  return code.ToString();
        }
    }

    #endregion
}
