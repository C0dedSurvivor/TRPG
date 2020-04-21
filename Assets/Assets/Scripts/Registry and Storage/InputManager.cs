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
    private static Dictionary<PlayerKeybinds, ComplexKeybinding> boundActions = new Dictionary<PlayerKeybinds, ComplexKeybinding>()
    {
        #region Map_Movement
        { PlayerKeybinds.MapMoveForward, new ComplexKeybinding(null, new List<KeyCode>(){ KeyCode.W }) },
        { PlayerKeybinds.MapMoveBack, new ComplexKeybinding(null, new List<KeyCode>(){ KeyCode.S }) },
        { PlayerKeybinds.MapMoveLeft, new ComplexKeybinding(null, new List<KeyCode>(){ KeyCode.A }) },
        { PlayerKeybinds.MapMoveRight, new ComplexKeybinding(null, new List<KeyCode>(){ KeyCode.D }) },
        { PlayerKeybinds.MapJump, new ComplexKeybinding(null, new List<KeyCode>(){ KeyCode.Space }) },
        { PlayerKeybinds.MapAdjustCameraDistance, new ComplexKeybinding(null, new List<KeyCode>(){ KeyCode.LeftControl, KeyCode.RightControl }) },
        #endregion
        #region UI
        { PlayerKeybinds.UIOpenPause, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.Escape }) },
        { PlayerKeybinds.UIOpenTeamPage, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.T }) },
        { PlayerKeybinds.UIOpenInventory, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.I }) },
        { PlayerKeybinds.UIConfirm, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.Return }) },
        { PlayerKeybinds.UIBack, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.Escape }) },
        { PlayerKeybinds.UIContinueText, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.Return }) },
        { PlayerKeybinds.UISkipText, new ComplexKeybinding(new List<KeyCode>(){ KeyCode.Return }) },
        #endregion
        #region Battle
        #endregion
    };

    /// <summary>
    /// Checks if the input conditions were met for a given action
    /// </summary>
    /// <param name="action">What action to check for</param>
    /// <returns>True if any of the trigger keys were pressed while the given conditions were met</returns>
    public static bool KeybindTriggered(PlayerKeybinds action)
    {
        foreach (KeyCode key in boundActions[action].mustHaveDown)
        {
            if (!Input.GetKey(key))
                return false;
        }
        foreach (KeyCode key in boundActions[action].mustHaveUp)
        {
            if (Input.GetKey(key))
                return false;
        }
        foreach (KeyCode key in boundActions[action].triggerKeyPressed)
        {
            if (Input.GetKeyDown(key))
                return true;
        }
        foreach (KeyCode key in boundActions[action].triggerKeyDown)
        {
            if (Input.GetKey(key))
                return true;
        }
        return false;
    }
}