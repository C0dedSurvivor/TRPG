using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TextEvent : BattleEventBase
{
    public string text;

    public TextEvent(string textToDisplay)
    {
        text = textToDisplay;
    }
}
