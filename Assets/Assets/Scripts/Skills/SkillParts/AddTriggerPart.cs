using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTriggerPart : SkillPartBase
{
    public TriggeredEffect effect;
    //The amount of times this effect can be triggered per battle
    public int maxTimesThisBattle;
    //The amount of turns that need to pass between effect activation, inclusive of the turn it can be used on. -1 if there is no limit
    public int turnCooldown;
    //The amount of turns this trigger exists for. -1 if there is no limit
    public int maxActiveTurns;

    public AddTriggerPart(TargettingType target, TriggeredEffect effect, int maxTimesThisBattle = -1, int turnCooldown = -1, int maxActiveTurns = -1, int chance = 100) : base(target, chance)
    {
        this.effect = effect;
        this.maxTimesThisBattle = maxTimesThisBattle;
        this.turnCooldown = turnCooldown;
        this.maxActiveTurns = maxActiveTurns;
    }
}
