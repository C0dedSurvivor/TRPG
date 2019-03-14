using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : StoredItem
{
    /// <summary>
    /// Keeps track of the battle-mutable effect limiters for each triggerable effect
    /// If restrictionType = 1, amount of times it's been used this battle
    /// If restrictionType = 2, amount of turns since this was last used
    /// </summary>
    List<int> effectCounts = new List<int>();

    public Equippable(string name) : base(name, 1)
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        //Creates a count info for each effect
        foreach (TriggeredEffect effect in itemBase.effects)
        {
            effectCounts.Add(0);
        }
    }

    /// <summary>
    /// Resets the restrictions at the start of the battle
    /// </summary>
    public void StartOfMatch()
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        for (int i = 0; i < effectCounts.Count; i++)
        {
            if (itemBase.effects[i].restrictionType == 1)
                effectCounts[i] = 0;
            if (itemBase.effects[i].restrictionType == 2)
                effectCounts[i] = int.MaxValue;
        }
    }

    /// <summary>
    /// Changes amount of turns since last used at the start of the turn if that is the necessary restriction
    /// </summary>
    public void StartOfTurn()
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        for (int i = 0; i < effectCounts.Count; i++)
        {
            if (itemBase.effects[i].restrictionType == 2)
                effectCounts[i]++;
        }
    }

    /// <summary>
    /// When the effect is activated, change count accordingly
    /// </summary>
    /// <param name="index">The index of the effect that wasa activated</param>
    public void Activated(int index)
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        if (itemBase.effects[index].restrictionType == 1)
            effectCounts[index]++;
        if (itemBase.effects[index].restrictionType == 2)
            effectCounts[index] = 0;
    }

    public List<SkillPartBase> GetTriggeredEffects(EffectTriggers trigger)
    {
        EquippableBase itemBase = Registry.ItemRegistry[name] as EquippableBase;
        List<SkillPartBase> list = new List<SkillPartBase>();
        for (int i = 0; i < effectCounts.Count; i++)
        {
            if(itemBase.effects[i].trigger == trigger && itemBase.effects[i].Activatable(effectCounts[i]))
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
