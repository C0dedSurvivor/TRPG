using System.Collections.Generic;

class ConnectedChanceEffect
{
    //The list of effects to execute if this is triggered
    public List<SkillPartBase> effects;
    //The chance of this effect being chosen
    public int triggerChance;

    public ConnectedChanceEffect(List<SkillPartBase> effects, int triggerChance)
    {
        this.effects = effects;
        this.triggerChance = triggerChance;
    }
}