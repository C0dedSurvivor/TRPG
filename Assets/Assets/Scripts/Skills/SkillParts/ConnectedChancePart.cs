using System.Collections.Generic;
using UnityEngine;

class ConnectedChancePart : SkillPartBase
{
    public List<ConnectedChanceEffect> effects;
    //What counts as a 100% chance for this weighting
    public int chanceOutOf;

    public ConnectedChancePart() { }

    public ConnectedChancePart(TargettingType target, List<ConnectedChanceEffect> effects, int chanceOutOf, int chance = 100) : base(target, chance)
    {
        this.effects = effects;
        this.chanceOutOf = chanceOutOf;
    }

    /// <summary>
    /// Generates a random value between 1 and the number of subdivisions (inclusive)
    /// It then checks for each effect if it lands on that effect, and if it lands on none it returns no effect
    /// </summary>
    /// <returns>The list of effects that should be triggered</returns>
    public List<SkillPartBase> ChooseEffect()
    {
        int randomSelector = Random.Range(0, chanceOutOf) + 1;
        Debug.Log("Connected chance random value: " + randomSelector);
        //Checks for each effect if it is the chosen effect
        foreach (ConnectedChanceEffect effect in effects)
        {
            if (randomSelector <= effect.triggerChance)
                return effect.effects;
            randomSelector -= effect.triggerChance;
        }
        //Returns no effect if the chosen number goes above the final effect
        return new List<SkillPartBase>();
    }
}