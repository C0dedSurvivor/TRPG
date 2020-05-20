using System.Collections.Generic;

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

    /// <summary>
    /// Adds an effect to be triggered by the given event
    /// </summary>
    /// <param name="effect">Effect to add</param>
    public void AddEffect(SkillPartBase effect)
    {
        effects.Add(effect);
    }
}
