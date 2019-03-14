using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggeredEffect
{
    //What event triggers this set of effects
    public EffectTriggers trigger;
    //The effects
    public List<SkillPartBase> effects = new List<SkillPartBase>();
    //0 = none, 1 = restricted amount of uses per battle, 2 = turn delay before reactivation
    public int restrictionType;
    //If restrictionType = 1, amount of times it's allowed to be used per battle
    //If restrictionType = 2, minimum delay between uses in turns, inclusive of the turn it can be used on
    public int restriction;

    public TriggeredEffect(EffectTriggers trigger, int restrictionType = 0, int restriction = 0, SkillPartBase effect = null)
    {
        this.trigger = trigger;
        this.restrictionType = restrictionType;
        this.restriction = restriction;
        if (effect != null)
            AddEffect(effect);
    }

    public void AddEffect(SkillPartBase effect)
    {
        effects.Add(effect);
    }

    public bool Activatable(int count)
    {
        //If it has reached its limit of activations per battle
        if (restrictionType == 1 && restriction <= count)
            return false;
        //If it is still on cooldown
        if (restrictionType == 2 && restriction > count)
            return false;
        return true;
    }
}
