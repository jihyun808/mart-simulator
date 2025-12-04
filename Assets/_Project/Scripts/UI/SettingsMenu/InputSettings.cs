// InputSettings.cs
using System;
using System.Collections.Generic;
using UnityEngine;

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
    Throw,
    Grab
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
        { Action.Throw, KeyCode.Mouse0 },
        { Action.Grab, KeyCode.Mouse1 },
    };

    public static float MouseSensitivity = 1.0f;
    public static bool InvertY = false;

    private const string PREF_MOUSE_SENS = "MouseSensitivity";
    private const string PREF_INVERT_Y = "InvertY";
    private const string PREF_KEY_PREFIX = "KEY_";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Load();
    }

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
        PlayerPrefs.Save();
    }

    public static void SaveInvertY(bool invert)
    {
        InvertY = invert;
        PlayerPrefs.SetInt(PREF_INVERT_Y, invert ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SaveKey(Action action, KeyCode code)
    {
        Keys[action] = code;
        string key = PREF_KEY_PREFIX + action.ToString();
        PlayerPrefs.SetInt(key, (int)code);
        PlayerPrefs.Save();
    }

    public static void ResetToDefaults()
    {
        Keys[Action.MoveForward] = KeyCode.W;
        Keys[Action.MoveBackward] = KeyCode.S;
        Keys[Action.MoveLeft] = KeyCode.A;
        Keys[Action.MoveRight] = KeyCode.D;
        Keys[Action.Jump] = KeyCode.Space;
        Keys[Action.Run] = KeyCode.LeftShift;
        Keys[Action.Crouch] = KeyCode.LeftControl;
        Keys[Action.Interact] = KeyCode.E;
        Keys[Action.Inventory1] = KeyCode.F1;
        Keys[Action.Inventory2] = KeyCode.F2;
        Keys[Action.Inventory3] = KeyCode.F3;
        Keys[Action.Inventory4] = KeyCode.F4;
        Keys[Action.ListToggle] = KeyCode.Tab;
        Keys[Action.Pause] = KeyCode.Escape;
        Keys[Action.Throw] = KeyCode.Mouse0;
        Keys[Action.Grab] = KeyCode.Mouse1;

        MouseSensitivity = 1.0f;
        InvertY = false;

        foreach (Action action in Enum.GetValues(typeof(Action)))
        {
            string key = PREF_KEY_PREFIX + action.ToString();
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.DeleteKey(PREF_MOUSE_SENS);
        PlayerPrefs.DeleteKey(PREF_INVERT_Y);
        PlayerPrefs.Save();
    }
}