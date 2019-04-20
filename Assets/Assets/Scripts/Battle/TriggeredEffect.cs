using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggeredEffect
{
    //What event triggers this set of effects
    public EffectTriggers trigger;
    //The effects
    public List<SkillPartBase> effects = new List<SkillPartBase>();

    public TriggeredEffect(EffectTriggers trigger, SkillPartBase effect = null)
    {
        this.trigger = trigger;
        if (effect != null)
            AddEffect(effect);
    }

    public void AddEffect(SkillPartBase effect)
    {
        effects.Add(effect);
    }
}
