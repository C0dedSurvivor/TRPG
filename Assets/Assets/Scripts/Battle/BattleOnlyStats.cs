using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection
{
    North,
    East,
    South,
    West
}

public class BattleOnlyStats
{
    public List<StatMod> modifierList;

    //Never do this again
    //public List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>> temporaryEffectList = new List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>>();

    /// <summary>
    /// A list of all temporary triggered effects put on a pawn for a battle
    /// </summary>
    public List<Pair<TriggeredEffect, TemporaryEffectData>> temporaryEffectList;

    public bool moved;

    public Vector2Int position;

    public FacingDirection facing;

    public BattleOnlyStats(int x, int y, BattlePawnBase pawn)
    {
        modifierList = new List<StatMod>();
        position = new Vector2Int(x, y);
        moved = false;
        facing = FacingDirection.North;

        temporaryEffectList = new List<Pair<TriggeredEffect, TemporaryEffectData>>();
        foreach (Equippable i in pawn.equipment)
        {
            if (i != null)
            {
                foreach (Pair<TriggeredEffect, TemporaryEffectData> effect in ((EquippableBase)Registry.ItemRegistry[i.Name]).effects)
                {
                    temporaryEffectList.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect.First,
                        new TemporaryEffectData(effect.Second.maxTimesThisBattle, effect.Second.turnCooldown, effect.Second.maxActiveTurns)));
                }
            }
        }
    }

    /// <summary>
    /// Gets the combined total of all stat mods that affect a single stat on this partcipant
    /// </summary>
    /// <param name="affectedStat">What stat to check for</param>
    /// <returns>A new statMod containing the combined values of all statMods affecting this pawn for the specified stat</returns>
    public StatMod GetStatMod(Stats affectedStat)
    {
        StatMod statMod = new StatMod(affectedStat, 0, 0, 0);

        foreach (StatMod s in modifierList)
        {
            if (s.affectedStat == affectedStat)
            {
                statMod.flatMod += s.flatMod;
                statMod.multMod += s.multMod - 1;
            }
        }
        statMod.multMod += 1;

        return statMod;
    }

    /// <summary>
    /// Adds the stat mod created from these parts to this pawn's list of stat mods
    /// </summary>
    /// <param name="affectedStat">What stat is changed</param>
    /// <param name="flatMod">What flat value to modify the stat by</param>
    /// <param name="multMod">What multiplier to apply to the stat</param>
    /// <param name="duration">How long the mod lasts</param>
    public void AddMod(Stats affectedStat, int flatMod, int multMod, int duration)
    {
        modifierList.Add(new StatMod(affectedStat, flatMod, multMod, duration));
    }

    /// <summary>
    /// Adds the stat mod to this pawn's list of stat mods
    /// </summary>
    /// <param name="mod">The mod to add</param>
    public void AddMod(StatMod mod)
    {
        modifierList.Add(mod);
    }

    /// <summary>
    /// Gets the list of effects triggered by the given event
    /// </summary>
    /// <param name="trigger">Event to check</param>
    public List<SkillPartBase> GetTriggeredEffects(EffectTriggers trigger)
    {
        List<SkillPartBase> list = new List<SkillPartBase>();

        for (int i = 0; i < temporaryEffectList.Count; i++)
        {
            if (temporaryEffectList[i].First.trigger == trigger)
            {
                //If it has not reached its limit of activations per battle
                if (temporaryEffectList[i].Second.Activatable())
                {
                    temporaryEffectList[i].Second.Activated();
                    foreach (SkillPartBase effect in temporaryEffectList[i].First.effects)
                    {
                        list.Add(effect);
                    }
                    //If it has reached its limit of activations per battle, remove it
                    if (temporaryEffectList[i].Second.OutOfUses())
                    {
                        temporaryEffectList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        return list;
    }

    public void AddTemporaryTrigger(TriggeredEffect effect, int maxTimesThisBattle, int turnCooldown, int maxActiveTurns)
    {
        Debug.Log("Adding a temporary effect");
        temporaryEffectList.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect, new TemporaryEffectData(maxTimesThisBattle, turnCooldown, maxActiveTurns)));
    }

    public void StartOfTurn()
    {
        foreach (Pair<TriggeredEffect, TemporaryEffectData> effect in temporaryEffectList)
        {
            effect.Second.StartOfTurn();
        }
    }

    /// <summary>
    /// Iterates through all stat changes and removes any that expire
    /// Also deals with voiding any effects that have run their course
    /// </summary>
    public void EndOfTurn()
    {
        for (int i = 0; i < modifierList.Count; i++)
        {
            modifierList[i].CountDown();
            if (modifierList[i].duration == 0)
            {
                modifierList.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < temporaryEffectList.Count; i++)
        {
            //If the effect has a turn limit and has reached that turn limit, remove it
            if (temporaryEffectList[i].Second.EndOfTurn())
            {
                temporaryEffectList.RemoveAt(i);
                i--;
            }
        }
    }
}
