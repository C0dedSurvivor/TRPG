using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : StoredItem
{
    /// <summary>
    /// Keeps track of the battle-mutable effect limiters for each triggerable effect
    /// First number is the times this was activated this battle
    /// Second number is the amount of turns since this was last used
    /// </summary>
    List<Pair<int, int>> effectCounts = new List<Pair<int, int>>();

    public Equippable(string name) : base(name, 1)
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        //Creates a count info for each effect
        foreach (TriggeredEffect effect in itemBase.effects)
        {
            effectCounts.Add(new Pair<int, int>(0, int.MaxValue));
        }
    }

    /// <summary>
    /// Resets the restrictions at the start of the battle
    /// </summary>
    public void StartOfMatch()
    {
        for (int i = 0; i < effectCounts.Count; i++)
        {
            effectCounts[i].First = 0;
            effectCounts[i].Second = int.MaxValue;
        }
    }

    /// <summary>
    /// Changes amount of turns since last used at the start of the turn if that is the necessary restriction
    /// </summary>
    public void StartOfTurn()
    {
        for (int i = 0; i < effectCounts.Count; i++)
        {
            if(effectCounts[i].Second < int.MaxValue)
                effectCounts[i].Second++;
        }
    }

    /// <summary>
    /// When the effect is activated, change count accordingly
    /// </summary>
    /// <param name="index">The index of the effect that wasa activated</param>
    public void Activated(int index)
    {
        effectCounts[index].First++;
        effectCounts[index].Second = 0;
    }

    public List<SkillPartBase> GetTriggeredEffects(EffectTriggers trigger)
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        List<SkillPartBase> list = new List<SkillPartBase>();
        for (int i = 0; i < effectCounts.Count; i++)
        {
            if(itemBase.effects[i].trigger == trigger && itemBase.effects[i].Activatable(effectCounts[i].First, effectCounts[i].Second))
            {
                foreach (SkillPartBase effect in itemBase.effects[i].effects)
                {
                    list.Add(effect);
                }
                Activated(i);
            }
        }
        return list;
    }
}
