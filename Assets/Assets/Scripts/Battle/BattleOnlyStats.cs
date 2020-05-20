using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The cardinal directions a pawn can be facing
/// </summary>
public enum FacingDirection
{
    North,
    East,
    South,
    West
}

/// <summary>
/// All stats given to a pawn that are only relevant for the duration of a battle
/// <see cref="BattlePawnBase"/>
/// </summary>
public class BattleOnlyStats
{
    //A list of stat mods currently affecting the pawn
    public List<StatMod> modifierList;

    //Never do this again
    //public List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>> temporaryEffectList = new List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>>();

    /// <summary>
    /// A list of all temporary triggered effects put on a pawn for a battle
    /// </summary>
    public List<Pair<TriggeredEffect, TemporaryEffectData>> temporaryEffectList;

    //Whether the pawn has already moved this turn
    public bool moved;

    //The position of the pawn in battle coordinates
    public Vector2Int position;

    //What direction the pawn's model is facing in
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
                foreach (AddTriggerPart effect in ((EquippableBase)Registry.ItemRegistry[i.Name]).effects)
                {
                    temporaryEffectList.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect.effect,
                        new TemporaryEffectData(effect.maxTimesThisBattle, effect.turnCooldown, effect.maxActiveTurns)));
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
                statMod.flatChange += s.flatChange;
                statMod.multiplier += s.multiplier - 1;
            }
        }
        statMod.multiplier += 1;

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

    /// <summary>
    /// Adds an effect that only exists for this battle with certain limitations on its triggering
    /// </summary>
    /// <param name="effect">The effect to add</param>
    /// <param name="maxTimesThisBattle">How many times this effect can be triggered this battle</param>
    /// <param name="turnCooldown">How long has to pass before the effect can be triggered again</param>
    /// <param name="maxActiveTurns">After how many turns can this effect no longer be triggered</param>
    public void AddTemporaryTrigger(TriggeredEffect effect, int maxTimesThisBattle, int turnCooldown, int maxActiveTurns)
    {
        Debug.Log("Adding a temporary effect");
        temporaryEffectList.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect, new TemporaryEffectData(maxTimesThisBattle, turnCooldown, maxActiveTurns)));
    }

    /// <summary>
    /// Triggers changes that happen at the start of a new turn
    /// </summary>
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
