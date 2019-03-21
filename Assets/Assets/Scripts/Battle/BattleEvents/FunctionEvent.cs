using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionEvent : BattleEventBase
{
    public delegate void DefaultType();

    public DefaultType function;

    public FunctionEvent(DefaultType function)
    {
        this.function = function;
    }
}
