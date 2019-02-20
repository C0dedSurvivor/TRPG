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
/// An item that ca be equipped to a pawn
/// </summary>
public class EquippableBase : ItemBase
{
    public int health;
    public int strength;
    public int defense;
    public int critChanceMod;

    //0 = weapon, 1 = helmet, 2 = chestplate, 3 = legs, 4 = boots, 5 = gloves, 6 = accessory
    public int equipSlot;

    /// <summary>
    /// Weapon: ID of it's weapon type
    /// Armor: Heavy, medium, light
    /// </summary>
    public int subType;

    //0 = physical, 1 = magical
    public int statType;

    public EquippableBase(int slot, int subtype, int stattype, int healthChange, int strengthChange, int defenseChange, int critChange, int sellPrice, string flavor = "") : base(1, sellPrice, flavor)
    {
        equipSlot = slot;
        subType = subtype;
        statType = stattype;
        health = healthChange;
        strength = strengthChange;
        defense = defenseChange;
        critChanceMod = critChange;
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