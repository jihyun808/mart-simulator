// ControlsSettings.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class KeyBindingWidget
{
    public Action action;
    public Button button;
    public TMP_Text label;
}

public class ControlsSettings : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertYToggle;

    [Header("Key Bindings")]
    [SerializeField] private List<KeyBindingWidget> keyBindingWidgets;

    private bool isRebinding = false;
    private Action currentAction;

    private void Awake()
    {
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

        foreach (var widget in keyBindingWidgets)
        {
            UpdateKeyLabel(widget);
            var capturedAction = widget.action;
            widget.button.onClick.AddListener(() => BeginRebind(capturedAction));
        }
    }

    private void Update()
    {
        if (!isRebinding) return;

        if (Input.anyKeyDown)
        {
            KeyCode pressedKey = DetectPressedKey();
            if (pressedKey != KeyCode.None)
            {
                ApplyRebind(pressedKey);
            }
        }
    }

    private void OnMouseSensitivityChanged(float value)
    {
        InputSettings.SaveMouseSensitivity(value);
    }

    private void OnInvertYChanged(bool invert)
    {
        InputSettings.SaveInvertY(invert);
    }

    private void BeginRebind(Action action)
    {
        if (isRebinding) return;

        isRebinding = true;
        currentAction = action;

        var widget = keyBindingWidgets.Find(w => w.action == action);
        if (widget != null && widget.label != null)
        {
            widget.label.text = "Press Key...";
        }
    }

    private KeyCode DetectPressedKey()
    {
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
        InputSettings.SaveKey(currentAction, newKey);

        var widget = keyBindingWidgets.Find(w => w.action == currentAction);
        if (widget != null)
        {
            UpdateKeyLabel(widget);
        }
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
            case KeyCode.LeftShift: return "Left Shift";
            case KeyCode.RightShift: return "Right Shift";
            case KeyCode.LeftControl: return "Left Ctrl";
            case KeyCode.RightControl: return "Right Ctrl";
            case KeyCode.Space: return "Space";
            case KeyCode.Mouse0: return "Left Click";
            case KeyCode.Mouse1: return "Right Click";
            default: return code.ToString();
        }
    }

    public void ResetToDefaults()
    {
        InputSettings.ResetToDefaults();

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = InputSettings.MouseSensitivity;

        if (invertYToggle != null)
            invertYToggle.isOn = InputSettings.InvertY;

        foreach (var widget in keyBindingWidgets)
        {
            UpdateKeyLabel(widget);
        }
    }
}