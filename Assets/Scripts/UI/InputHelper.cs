using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Input Helper - מטפל בכל סוגי ה-Input (Legacy ו-Input System)
/// פותר את הבעיה עם Input System vs Legacy Input
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// בדיקה אם מקש נלחץ - עובד עם שני המערכות
    /// </summary>
    public static bool GetKeyDown(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        return GetKeyDownInputSystem(keyCode);
#else
            return Input.GetKeyDown(keyCode);
#endif
    }

    /// <summary>
    /// בדיקה אם מקש נלחץ - עובד עם שני המערכות
    /// </summary>
    public static bool GetKey(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        return GetKeyInputSystem(keyCode);
#else
            return Input.GetKey(keyCode);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static bool GetKeyDownInputSystem(KeyCode keyCode)
    {
        var key = ConvertKeyCodeToKey(keyCode);
        if (key != Key.None)
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }
        return false;
    }

    private static bool GetKeyInputSystem(KeyCode keyCode)
    {
        var key = ConvertKeyCodeToKey(keyCode);
        if (key != Key.None)
        {
            return Keyboard.current != null && Keyboard.current[key].isPressed;
        }
        return false;
    }

    private static Key ConvertKeyCodeToKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Space: return Key.Space;
            case KeyCode.F1: return Key.F1;
            case KeyCode.F2: return Key.F2;
            case KeyCode.F3: return Key.F3;
            case KeyCode.F4: return Key.F4;
            case KeyCode.F5: return Key.F5;
            case KeyCode.F6: return Key.F6;
            case KeyCode.F7: return Key.F7;
            case KeyCode.F8: return Key.F8;
            case KeyCode.F9: return Key.F9;
            case KeyCode.F10: return Key.F10;
            case KeyCode.F11: return Key.F11;
            case KeyCode.F12: return Key.F12;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightControl: return Key.RightCtrl;
            case KeyCode.LeftAlt: return Key.LeftAlt;
            case KeyCode.RightAlt: return Key.RightAlt;
            case KeyCode.A: return Key.A;
            case KeyCode.B: return Key.B;
            case KeyCode.C: return Key.C;
            case KeyCode.D: return Key.D;
            case KeyCode.E: return Key.E;
            case KeyCode.F: return Key.F;
            case KeyCode.G: return Key.G;
            case KeyCode.H: return Key.H;
            case KeyCode.I: return Key.I;
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.L: return Key.L;
            case KeyCode.M: return Key.M;
            case KeyCode.N: return Key.N;
            case KeyCode.O: return Key.O;
            case KeyCode.P: return Key.P;
            case KeyCode.Q: return Key.Q;
            case KeyCode.R: return Key.R;
            case KeyCode.S: return Key.S;
            case KeyCode.T: return Key.T;
            case KeyCode.U: return Key.U;
            case KeyCode.V: return Key.V;
            case KeyCode.W: return Key.W;
            case KeyCode.X: return Key.X;
            case KeyCode.Y: return Key.Y;
            case KeyCode.Z: return Key.Z;
            case KeyCode.Alpha0: return Key.Digit0;
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Alpha8: return Key.Digit8;
            case KeyCode.Alpha9: return Key.Digit9;
            default: return Key.None;
        }
    }
#endif
}
