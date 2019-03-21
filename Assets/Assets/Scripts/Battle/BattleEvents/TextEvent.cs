using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TextEvent : BattleEventBase
{
    public string text;

    public TextEvent(string textToDisplay)
    {
        text = textToDisplay;
    }
}
