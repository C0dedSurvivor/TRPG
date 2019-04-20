using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum MoveTriggers
{
    StartOfTurn,
    PassOver,
    StopOnTile,
    EndOfTurn
}

public class TileEffects
{
    public Dictionary<MoveTriggers, ExecuteEffectEvent> effects;

    public TileEffects(Dictionary<MoveTriggers, ExecuteEffectEvent> effects)
    {
        this.effects = effects;
    }

    public bool Contains(MoveTriggers trigger)
    {
        return effects.ContainsKey(trigger);
    }
}