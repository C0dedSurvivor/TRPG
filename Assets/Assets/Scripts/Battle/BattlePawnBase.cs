using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class BattlePawnBase
{
    public string name;

    public Dictionary<Stats, int> stats = new Dictionary<Stats, int>();

    public int cHealth;

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

    //x = skill tree id, y = spell id
    public Dictionary<int, Dictionary<int, SkillInfo>> skillTreeList = new Dictionary<int, Dictionary<int, SkillInfo>>();

    //The stats that only matter in a battle
    public BattleOnlyStats tempStats;

    //Player creator
    public BattlePawnBase(){ }

    //Enemy creator
    public BattlePawnBase(int x, int y, int mT, string name)
    {
        this.name = name;
        moveType = mT;
        stats.Add(Stats.MaxHealth, 20 + mT);
        stats.Add(Stats.Attack, 20 + mT);
        stats.Add(Stats.Defense, 10 + mT);
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
        cHealth = stats[Stats.MaxHealth];
        equippedWeapon = new Equippable("Wooden Sword");

        //Grab all the skill trees and skills for this pawn
        List<int> treeList = GameStorage.GetPlayerSkillList(name);
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

        tempStats = new BattleOnlyStats(x, y, this);
    }

    /// <summary>
    /// Adds the specified status effect to this pawn
    /// </summary>
    /// <param name="status">Status effect to add</param>
    /// <param name="duration">The duration of the status effect, -1 if time is not the condition on which it is removed</param>
    public void AddStatusEffect(Statuses status, int duration = -1)
    {
        statusList.Add(status, duration);

        if (tempStats != null && Registry.StatusEffectRegistry[status].freezeOnAffliction)
            tempStats.moved = true;
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

        if (tempStats != null)
        {
            StatMod s = tempStats.GetStatMod(stat);

            value += s.flatMod;
            value = Mathf.RoundToInt(value * s.multMod);
        }

        if (stat == Stats.MaxMove && value < 0)
            value = 0;

        return value;
    }

    /// <summary>
    /// Changes the current health of the pawn to match if the max health is decreased
    /// </summary>
    /// <param name="prevMax">The previous max hp</param>
    public void CheckHealthChange(int prevMax)
    {
        if (cHealth > GetEffectiveStat(Stats.MaxHealth))
            cHealth = GetEffectiveStat(Stats.MaxHealth);
        if (cHealth == prevMax)
            cHealth = GetEffectiveStat(Stats.MaxHealth);
    }

    /// <summary>
    /// Returns true if sent value matches the code of a tile type this participant can move over
    /// </summary>
    /// <param name="tileValue">Tile type to check for</param>
    public bool ValidMoveTile(int tileValue)
    {
        return Registry.MovementRegistry[moveType].passableTiles.ContainsKey((BattleTiles)tileValue);
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

    /// <summary>
    /// Deals with end of turn status effects
    /// Also deals with voiding any effects that have run their course
    /// </summary>
    public void EndOfTurn()
    {
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
