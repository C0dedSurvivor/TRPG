using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum MoveTriggers
{
    StartOfTurn,
    PassOver,
    StopOnTile,
    EndOfTurn
}

public class TileType
{
    public string name { get; set; }
    public string flavorText;
    public bool blocksMeleeAttacks;
    public bool blocksRangedAttacks;
    public SkillPartBase startOfTurn;
    public SkillPartBase passOver;
    public SkillPartBase stopOnTile;
    public SkillPartBase endOfTurn;

    public ExecuteEffectEvent this[MoveTriggers trigger]
    {
        get
        {
            switch (trigger)
            {
                case MoveTriggers.StartOfTurn: return new ExecuteEffectEvent(startOfTurn, null, null);
                case MoveTriggers.PassOver: return new ExecuteEffectEvent(passOver, null, null);
                case MoveTriggers.StopOnTile: return new ExecuteEffectEvent(stopOnTile, null, null);
                case MoveTriggers.EndOfTurn: return new ExecuteEffectEvent(endOfTurn, null, null);
            }
            return null;
        }
    }

    public TileType(string name, string flavorText, bool blocksMelee = false, bool blocksRanged = false, SkillPartBase startOfTurn = null, 
        SkillPartBase passOver = null, SkillPartBase stopOnTile = null, SkillPartBase endOfTurn = null)
    {
        this.name = name;
        this.flavorText = flavorText;
        blocksMeleeAttacks = blocksMelee;
        blocksRangedAttacks = blocksRanged;
        this.startOfTurn = startOfTurn;
        this.passOver = passOver;
        this.stopOnTile = stopOnTile;
        this.endOfTurn = endOfTurn;
    }

    public bool Contains(MoveTriggers trigger)
    {
        return trigger == MoveTriggers.StartOfTurn && startOfTurn != null ||
            trigger == MoveTriggers.PassOver && passOver != null ||
            trigger == MoveTriggers.StopOnTile && stopOnTile != null ||
            trigger == MoveTriggers.EndOfTurn && endOfTurn != null;
    }
}