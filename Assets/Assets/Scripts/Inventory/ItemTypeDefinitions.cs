﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase
{
    int maxStack;
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

public class BattleItemBase : ItemBase
{
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