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

public enum Stats
{
    MaxHealth,
    Attack,
    Defense,
    MagicAttack,
    MagicDefense,
    CritChance,
    MaxMove,
    BasicAttackLifesteal,
    SpellLifesteal,
    BasicAttackEffectiveness,
    BasicAttackReceptiveness,
    SpellDamageEffectiveness,
    SpellDamageReceptiveness,
    HealingEffectiveness,
    HealingReceptiveness
}

public class SkillInfo
{
    public bool unlocked;
    public int level;

    public SkillInfo()
    {
        unlocked = false;
        level = 1;
    }

    public SkillInfo(bool unlock, int lvl)
    {
        unlocked = unlock;
        level = lvl;
    }
}

public class BattleParticipant
{
    public string name;

    public Dictionary<Stats, int> stats = new Dictionary<Stats, int>();

    public int cHealth;

    public List<StatMod> modifierList = new List<StatMod>();
    public StatusEffectList statusList = new StatusEffectList();

    //1 = slow, 2 = walking, 3 = riding
    public int moveType = 1;

    //0 = weapon, 1 = helmet, 2 = chestplate, 3 = legs, 4 = boots, 5 = gloves, 6 = accessory 1, 7 = accessory 2
    public Equippable[] equipment = new Equippable[8];

    public Equippable equippedWeapon
    {
        get
        {
            return equipment[0];
        }
        set
        {
            equipment[0] = value;
        }
    }

    //Never do this again
    //public List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>> temporaryEffectList = new List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>>();

    /// <summary>
    /// A list of all temporary triggered effects put on a pawn for a battle
    /// </summary>
    public List<Pair<TriggeredEffect, TemporaryEffectData>> temporaryEffectList = new List<Pair<TriggeredEffect, TemporaryEffectData>>();

    public Vector2Int position;

    public bool moved;

    //x = skill tree id, y = spell id
    public Dictionary<int, Dictionary<int, SkillInfo>> skillTreeList = new Dictionary<int, Dictionary<int, SkillInfo>>();

    public FacingDirection facing = FacingDirection.North;

    public BattleParticipant(string name)
    {
        this.name = name;
    }

    //Enemy creator
    public BattleParticipant(int x, int y, int mT, string name)
    {
        this.name = name;
        position.x = x;
        position.y = y;
        moveType = mT;
        stats.Add(Stats.MaxHealth, 1);
        stats.Add(Stats.Attack, 15 + mT);
        stats.Add(Stats.Defense, 15 + mT);
        stats.Add(Stats.MagicAttack, 15 + mT);
        stats.Add(Stats.MagicDefense, 15 + mT);
        stats.Add(Stats.CritChance, 15 + mT);
        stats.Add(Stats.MaxMove, Registry.MovementRegistry[moveType].moveSpeed);
        stats.Add(Stats.BasicAttackLifesteal, 0);
        stats.Add(Stats.SpellLifesteal, 0);
        stats.Add(Stats.BasicAttackEffectiveness, 100);
        stats.Add(Stats.SpellDamageEffectiveness, 100);
        stats.Add(Stats.BasicAttackReceptiveness, 100);
        stats.Add(Stats.SpellDamageReceptiveness, 100);
        stats.Add(Stats.HealingEffectiveness, 100);
        stats.Add(Stats.HealingReceptiveness, 100);
        cHealth = GetEffectiveStat(Stats.MaxHealth);
        equippedWeapon = new Equippable("Wooden Sword");

        //Grab all the skill trees and skills for this pawn
        List<int> treeList = GameStorage.GetPlayerSkillList("");
        foreach (int tree in treeList)
        {
            skillTreeList.Add(tree, new Dictionary<int, SkillInfo>());
            foreach (int skill in GameStorage.skillTreeList[tree].Keys)
            {
                skillTreeList[tree].Add(skill, new SkillInfo());
                if (GameStorage.skillTreeList[tree][skill].dependencies.Count == 0)
                    skillTreeList[tree][skill].unlocked = true;
            }
        }
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
    /// Adds the specified status effect to this pawn
    /// </summary>
    /// <param name="status">Status effect to add</param>
    /// <param name="duration">The duration of the status effect, -1 if time is not the condition on which it is removed</param>
    public void AddStatusEffect(Statuses status, int duration = -1)
    {
        statusList.Add(status, duration);

        if (Registry.StatusEffectRegistry[status].freezeOnAffliction)
            moved = true;
    }

    /// <summary>
    /// Removes the specified status effect from this pawn
    /// </summary>
    /// <param name="status">The status effect to remove</param>
    public void RemoveStatusEffect(Statuses status)
    {
        if (statusList.Contains(status))
        {
            statusList.Remove(status);
        }
    }

    /// <summary>
    /// Gets the combined total of all stat mods that affect a single stat on this partcipant
    /// </summary>
    /// <param name="affectedStat">What stat to check for</param>
    /// <returns>A new statMod containing the combined values of all statMods affecting this pawn for the specified stat</returns>
    public StatMod GetStatMod(Stats affectedStat)
    {
        StatMod statMod = new StatMod(affectedStat, 0, 1, 0);

        foreach (StatMod s in modifierList)
        {
            if (s.affectedStat == affectedStat)
            {
                statMod.flatMod += s.flatMod;
                statMod.multMod += s.multMod;
            }
        }

        return statMod;
    }

    //
    /// <summary>
    /// Some weapons have different effects at different distances from their target
    /// </summary>
    /// <param name="dist">Distance from the target</param>
    /// <returns>The multiplier at that distance, default 1.0</returns>
    public WeaponStatsAtRange GetWeaponStatsAtDistance(Vector2Int dist)
    {
        //If nothing is equipped, a bare-knuckle smackdown is required
        return Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[equippedWeapon != null ? equippedWeapon.Name : "Fists"]).subType].GetStatsAtRange(dist);
    }

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
    /// Gets the total for a stat including its base value and all applicable modifiers
    /// </summary>
    /// <param name="stat">What stat to grab</param>
    /// <returns></returns>
    public int GetEffectiveStat(Stats stat)
    {
        int value = stats[stat];
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                EquippableBase temp = ((EquippableBase)Registry.ItemRegistry[i.Name]);
                if(temp.stats.ContainsKey(stat))
                    value += temp.stats[stat];
            }
        }

        StatMod s = GetStatMod(stat);

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        if (stat == Stats.MaxMove && value < 0)
            value = 0;

        return value;
    }

    /// <summary>
    /// Returns true if sent value matches the code of a tile type this participant can move over
    /// </summary>
    /// <param name="tileValue">Tile type to check for</param>
    public bool ValidMoveTile(int tileValue)
    {
        return Registry.MovementRegistry[moveType].passableTiles.Contains((BattleTiles)tileValue);
    }

    /// <summary>
    /// Heals the pawn, making sure it isn't healed past its max health
    /// </summary>
    /// <param name="healAmount">Max amount of health to gain</param>
    public int Heal(int healAmount)
    {
        int previous = cHealth;
        cHealth = Mathf.Clamp(cHealth + healAmount, 0, GetEffectiveStat(Stats.MaxHealth));
        return cHealth - previous;
    }

    /// <summary>
    /// Deals damage to the pawn, makes sure it isn't overkilled
    /// Also dispells effects that are removed by dealing damage
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public int Damage(int damage)
    {
        if (damage > 0 && statusList.Contains(Statuses.Sleep))
            statusList.Remove(Statuses.Sleep);
        int trueDamage = GetDamage(damage);
        cHealth -= trueDamage;
        return trueDamage;
    }

    public int GetDamage(int damage)
    {
        return Mathf.Min(damage, cHealth);
    }

    public void AddTemporaryTrigger(TriggeredEffect effect, int maxTimesThisBattle, int turnCooldown, int maxActiveTurns)
    {
        temporaryEffectList.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect, new TemporaryEffectData(maxTimesThisBattle, turnCooldown, maxActiveTurns)));
    }

    public void StartOfMatch()
    {
        moved = false;
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                foreach(Pair<TriggeredEffect, TemporaryEffectData> effect in ((EquippableBase)Registry.ItemRegistry[i.Name]).effects)
                {
                    temporaryEffectList.Add(new Pair<TriggeredEffect, TemporaryEffectData>(effect.First, 
                        new TemporaryEffectData(effect.Second.maxTimesThisBattle, effect.Second.turnCooldown, effect.Second.maxActiveTurns)));
                }
            }
        }
        temporaryEffectList = new List<Pair<TriggeredEffect, TemporaryEffectData>>();
    }

    public void StartOfTurn()
    {
        foreach (Pair<TriggeredEffect, TemporaryEffectData> effect in temporaryEffectList)
        {
            effect.Second.StartOfTurn();
        }
    }

    /// <summary>
    /// Iterates through all stat changes and removes any that expire and deals with end of turn status effects
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
        
        statusList.EndOfTurn();
    }

    /// <summary>
    /// Can the pawn move this turn
    /// </summary>
    public bool CanMove()
    {
        return !statusList.Contains(Statuses.Sleep) && cHealth > 0;
    }
}
