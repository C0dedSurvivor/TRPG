using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A basic item
/// </summary>
public class ItemBase
{
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
    public List<Pair<TriggeredEffect, TemporaryEffectData>> effects = new List<Pair<TriggeredEffect, TemporaryEffectData>>();

    public EquippableBase(int slot, int subtype, int sellPrice, string flavor = "", int health = 0, int pAtk = 0, int pDef = 0, int mAtk = 0, int mDef = 0, int critChance = 0, 
        int speed = 0, int basicAttackLifesteal = 0, int spellLifesteal = 0, int basicAttackEffectivenessMod = 0, int spellDamageEffectivenessMod = 0, int basicAttackReceptivenessMod = 0, 
        int spellDamageReceptivenessMod = 0, int healingEffectivenessMod = 0, int healingReceptivenessMod = 0) : base(1, sellPrice, flavor)
    {
        equipSlot = slot;
        subType = subtype;
        stats.Add(Stats.MaxHealth, health);
        stats.Add(Stats.Attack, pAtk);
        stats.Add(Stats.Defense, pDef);
        stats.Add(Stats.MagicAttack, mAtk);
        stats.Add(Stats.MagicDefense, mDef);
        stats.Add(Stats.CritChance, critChance);
        stats.Add(Stats.MaxMove, speed);
        stats.Add(Stats.BasicAttackLifesteal, basicAttackLifesteal);
        stats.Add(Stats.SpellLifesteal, spellLifesteal);
        stats.Add(Stats.BasicAttackEffectiveness, basicAttackEffectivenessMod);
        stats.Add(Stats.BasicAttackReceptiveness, basicAttackReceptivenessMod);
        stats.Add(Stats.SpellDamageEffectiveness, spellDamageEffectivenessMod);
        stats.Add(Stats.SpellDamageReceptiveness, spellDamageReceptivenessMod);
        stats.Add(Stats.HealingEffectiveness, healingEffectivenessMod);
        stats.Add(Stats.HealingReceptiveness, healingReceptivenessMod);
    }

    public void AddEffect(TriggeredEffect effect, int maxTimesThisBattle = -1, int turnCooldown = -1, int maxActiveTurns = -1)
    {
        effects.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect, new TemporaryEffectData(maxTimesThisBattle, turnCooldown, maxActiveTurns)));
    }
}

/// <summary>
/// An item that has an effect when used during battle
/// </summary>
public class BattleItemBase : ItemBase
{
    //Is this item can be used outside of battle
    public bool usableOutOfBattle;

    public List<SkillPartBase> partList = new List<SkillPartBase>();

    public BattleItemBase(bool outOfBattleUse, List<SkillPartBase> effects, int maxStack, int sellPrice, string flavor = "") : base(maxStack, sellPrice, flavor)
    {
        usableOutOfBattle = outOfBattleUse;
        foreach(SkillPartBase part in effects)
        {
            partList.Add(part);
        }
    }
}