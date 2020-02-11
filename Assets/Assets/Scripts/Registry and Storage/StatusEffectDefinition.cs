using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum CountdownType
{
    Turns,
    Uses,
    None
}

/// <summary>
/// All of the information pertaining to a specific status effect type
/// </summary>
public struct StatusEffectDefinition
{
    public string name { get; set; }
    //What limit there is on this effect, if any
    public CountdownType limit;
    //Does this effect persist between battles
    public bool persists;
    //The percent chance that this effect gets removed at the end of a turn
    public float endOfTurnRemoveChance;
    //If getting this effect stops you from being able to move the turn you get it
    public bool freezeOnAffliction;

    public StatusEffectDefinition(string name, CountdownType limit, bool persists, bool freezeOnAffliction, float endOfTurnRemoveChance = 0)
    {
        this.name = name;
        this.limit = limit;
        this.persists = persists;
        this.endOfTurnRemoveChance = endOfTurnRemoveChance;
        this.freezeOnAffliction = freezeOnAffliction;
    }
}