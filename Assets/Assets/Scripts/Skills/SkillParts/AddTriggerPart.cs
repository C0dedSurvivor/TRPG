using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTriggerPart : SkillPartBase
{
    public TriggeredEffect effect;
    //The amount of turns this trigger exists for, 0 if there is no limit
    public int turnLimit;

    public AddTriggerPart(TargettingType target, TriggeredEffect effect, int turnLimit = 0, int chance = 100) : base(target, chance)
    {
        this.effect = effect;
        this.turnLimit = turnLimit;
    }
}
