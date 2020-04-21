using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All possible actions that can be triggered by a key press
/// </summary>
public enum PlayerKeybinds
{
    MapMoveForward,
    MapMoveBack,
    MapMoveLeft,
    MapMoveRight,
    MapJump,
    MapAdjustCameraDistance,
    UIOpenPause,
    UIOpenTeamPage,
    UIOpenInventory,
    UIConfirm,
    UIBack,
    UIContinueText,
    UISkipText
}

class InputManager
{
    /// <summary>
    /// Binds the actions to the keys that trigger them
    /// </summary>
    private static Dictionary<PlayerKeybinds, List<KeyCode>> boundActions = new Dictionary<PlayerKeybinds, List<KeyCode>>()
    {
        #region Map_Movement
        { PlayerKeybinds.MapMoveForward, new List<KeyCode>{ KeyCode.W } },
        { PlayerKeybinds.MapMoveBack, new List<KeyCode>{ KeyCode.S } },
        { PlayerKeybinds.MapMoveLeft, new List<KeyCode>{ KeyCode.A } },
        { PlayerKeybinds.MapMoveRight, new List<KeyCode>{ KeyCode.D } },
        { PlayerKeybinds.MapJump, new List<KeyCode>{ KeyCode.Space } },
        { PlayerKeybinds.MapAdjustCameraDistance, new List<KeyCode>{ KeyCode.LeftControl, KeyCode.RightControl } },
        #endregion
        #region UI
        { PlayerKeybinds.UIOpenPause, new List<KeyCode>{ KeyCode.Escape } },
        { PlayerKeybinds.UIOpenTeamPage, new List<KeyCode>{ KeyCode.T } },
        { PlayerKeybinds.UIOpenInventory, new List<KeyCode>{ KeyCode.I } },
        { PlayerKeybinds.UIConfirm, new List<KeyCode>{ KeyCode.Return } },
        { PlayerKeybinds.UIBack, new List<KeyCode>{ KeyCode.Escape } },
        { PlayerKeybinds.UIContinueText, new List<KeyCode>{ KeyCode.Return } },
        { PlayerKeybinds.UISkipText, new List<KeyCode>{ KeyCode.Return } },
        #endregion
        #region Battle
        #endregion
    };

    /// <summary>
    /// Checks if any of the keycodes bound to the given action were pressed this frame
    /// </summary>
    /// <param name="action">What action to check for</param>
    /// <returns>True if any of the bound keys were pressed this turn</returns>
    public static bool BoundKeyPressed(PlayerKeybinds action)
    {
        foreach (KeyCode key in boundActions[action])
        {
            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if any of the keycodes bound to the given action are currently pressed
    /// </summary>
    /// <param name="action">What action to check for</param>
    /// <returns>True if any of the bound keys are pressed</returns>
    public static bool BoundKeyDown(PlayerKeybinds action)
    {
        foreach (KeyCode key in boundActions[action])
        {
            if (Input.GetKey(key))
                return true;
        }
        return false;
    }
}