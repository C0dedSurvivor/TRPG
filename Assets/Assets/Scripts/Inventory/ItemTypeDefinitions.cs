using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A basic item
/// </summary>
public class ItemBase
{
    public string name { get; set; }
    //Max amount the player can hold
    int maxStack;
    //How much one of this item sells for
    int sellAmount;
    string flavorText;

    public int MaxStack
    {
        get
        {
            return maxStack;
        }
    }

    public int SellAmount
    {
        get
        {
            return sellAmount;
        }
    }

    public string FlavorText
    {
        get
        {
            return flavorText;
        }
    }

    public ItemBase(int maxstack, int sell, string flavor = "")
    {
        maxStack = maxstack;
        sellAmount = sell;
        flavorText = flavor;
    }
}

/// <summary>
/// An item that can be equipped to a pawn
/// </summary>
public class EquippableBase : ItemBase
{
    public Dictionary<Stats, int> stats = new Dictionary<Stats, int>();

    //0 = weapon, 1 = helmet, 2 = chestplate, 3 = legs, 4 = boots, 5 = gloves, 6 = accessory
    public int equipSlot;

    /// <summary>
    /// Weapon: ID of it's weapon type
    /// Armor: Heavy, medium, light
    /// </summary>
    public int subType;

    /// <summary>
    /// Keeps track of the battle-mutable effect limiters for each triggerable effect
    /// The TemporaryEffectData here should never be modified
    /// </summary>
    public List<AddTriggerPart> effects = new List<AddTriggerPart>();

    public int TotalStats
    {
        get
        {
            int i = 0;
            foreach(Stats stat in stats.Keys)
            {
                i += stats[stat];
            }
            return i;
        }
    }

    public EquippableBase(int slot, int subtype, int sellPrice, string flavor, Dictionary<Stats, int> stats) : base(1, sellPrice, flavor)
    {
        equipSlot = slot;
        subType = subtype;
        this.stats = stats;
    }

    public void AddEffect(TriggeredEffect effect, int maxTimesThisBattle = -1, int turnCooldown = -1, int maxActiveTurns = -1)
    {
        effects.Add(new AddTriggerPart(TargettingType.Self, effect, maxTimesThisBattle, turnCooldown, maxActiveTurns));
    }
}

/// <summary>
/// An item that has an effect when used during battle
/// </summary>
public class BattleItemBase : ItemBase
{
    public TargettingType targetType;

    //Is this item can be used outside of battle
    public bool usableOutOfBattle;

    public List<SkillPartBase> partList = new List<SkillPartBase>();

    public BattleItemBase(TargettingType targetType, bool outOfBattleUse, List<SkillPartBase> effects, int maxStack, int sellPrice, string flavor = "") : base(maxStack, sellPrice, flavor)
    {
        this.targetType = targetType;
        usableOutOfBattle = outOfBattleUse;
        if (effects != null)
        {
            foreach (SkillPartBase part in effects)
            {
                partList.Add(part);
            }
        }
    }
}