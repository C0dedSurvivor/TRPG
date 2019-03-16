using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggeredEffect
{
    //What event triggers this set of effects
    public EffectTriggers trigger;
    //The effects
    public List<SkillPartBase> effects = new List<SkillPartBase>();
    //The maximum amount of times this effect can be used per battle, 0 if there is no limit
    public int maxTimesPerBattle;
    //Minimum delay between uses in turns, inclusive of the turn it can be used on. 0 if no limit
    public int delayBetweenUses;

    public TriggeredEffect(EffectTriggers trigger, int maxTimesPerBattle = 0, int delayBetweenUses = 0, SkillPartBase effect = null)
    {
        this.trigger = trigger;
        this.maxTimesPerBattle = maxTimesPerBattle;
        this.delayBetweenUses = delayBetweenUses;
        if (effect != null)
            AddEffect(effect);
    }

    public void AddEffect(SkillPartBase effect)
    {
        effects.Add(effect);
    }

    public bool Activatable(int usesThisBattle, int turnsSinceLastUse)
    {
        //If it has reached its limit of activations per battle
        if (maxTimesPerBattle > 0 && usesThisBattle >= maxTimesPerBattle)
            return false;
        //If it is still on cooldown
        if (delayBetweenUses > turnsSinceLastUse)
            return false;
        return true;
    }
}
