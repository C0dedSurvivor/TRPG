using System.Collections.Generic;
using UnityEngine;

class ComplexKeybinding
{
    //The keybinding will trigger if any of these keys were pressed this frame and the "must" conditions are met
    public List<KeyCode> triggerKeyPressed = new List<KeyCode>();
    //The keybinding will trigger if any of these keys are down this frame and the "must" conditions are met
    public List<KeyCode> triggerKeyDown = new List<KeyCode>();
    //All of these keys must be up for any of the triggers to work
    public List<KeyCode> mustHaveUp = new List<KeyCode>();
    //All of these keys must be down for any of the triggers to work
    public List<KeyCode> mustHaveDown = new List<KeyCode>();

    public ComplexKeybinding(List<KeyCode> triggerKeyPressed = null, List<KeyCode> triggerKeyDown = null, List<KeyCode> mustHaveUp = null, List<KeyCode> mustHaveDown = null)
    {
        if(triggerKeyPressed != null)
            this.triggerKeyPressed = triggerKeyPressed;
        if (triggerKeyDown != null)
            this.triggerKeyDown = triggerKeyDown;
        if (mustHaveUp != null)
            this.mustHaveUp = mustHaveUp;
        if (mustHaveDown != null)
            this.mustHaveDown = mustHaveDown;
    }
}