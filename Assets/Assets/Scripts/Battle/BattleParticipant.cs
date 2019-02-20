using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct statMod
{
    //atk, def, matk, mdef, crit, move
    public string affectedStat;
    public int flatMod;
    public int multMod;
    public int duration;

    public statMod(string aStat, int flat, int mult, int dur)
    {
        affectedStat = aStat;
        flatMod = flat;
        multMod = mult;
        duration = dur;
    }

    public void countDown()
    {
        duration--;
    }
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

public class BattleParticipant {
    public string name;

    public int attack;
    public int defense;
    public int mAttack;
    public int mDefense;
    public int critChance;
    public int cHealth;
    public int mHealth;

    public List<statMod> modifierList = new List<statMod>();
    public StatusEffectList statusList = new StatusEffectList();

    //1 = slow, 2 = walking, 3 = riding
    public int moveType = 1;

    //0 = weapon, 1 = helmet, 2 = chestplate, 3 = legs, 4 = boots, 5 = gloves, 6 = accessory 1, 7 = accessory 2
    public string[] equipment = new string[8];

    public string equippedWeapon
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

    public Vector2Int position;

    public bool moved;

    //x = skill tree id, y = spell id
    public Dictionary<int, Dictionary<int, SkillInfo>> skillTreeList = new Dictionary<int, Dictionary<int, SkillInfo>>();

    public BattleParticipant(string name) {
        this.name = name;
    }

    //Enemy creator
    public BattleParticipant(int x, int y, int mT)
    {
        position.x = x;
        position.y = y;
        moveType = mT;
        attack = 15 + mT;
        defense = 15 + mT;
        mAttack = 15 + mT;
        mDefense = 15 + mT;
        critChance = 15 + mT;
        mHealth = 1;
        cHealth = mHealth;
        equippedWeapon = "Wooden Sword";

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
    public void AddMod(string affectedStat, int flatMod, int multMod, int duration)
    {
        modifierList.Add(new statMod(affectedStat, flatMod, multMod, duration));
    }

    /// <summary>
    /// Adds the stat mod to this pawn's list of stat mods
    /// </summary>
    /// <param name="mod">The mod to add</param>
    public void AddMod(statMod mod)
    {
        modifierList.Add(mod);
    }

    /// <summary>
    /// Adds the specified status effect to this pawn
    /// </summary>
    /// <param name="status">Status effect to add</param>
    /// <param name="duration">The duration of the status effect, -1 if time is not the condition on which it is removed</param>
    public void AddStatusEffect(string status, int duration = -1)
    {
        string s;
        if (status.IndexOf(" ") != -1)
        {
            s = status.Substring(0, status.IndexOf(" "));
        }
        else
        {
            s = status;
        }

        if (Registry.StatusEffectRegistry[s].stackable || !statusList.Contains(status))
        {
            statusList.Add(status, duration);
        }
        else if(statusList.Contains(status) && Registry.StatusEffectRegistry[s].refreshOnDuplication)
        {
            statusList.Refresh(status, duration);
        }

        if (s.CompareTo("sleep") == 0)
            moved = true;
    }

    /// <summary>
    /// Removes the specified status effect from this pawn
    /// </summary>
    /// <param name="status">The status effect to remove</param>
    public void RemoveStatusEffect(string status)
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
    public statMod GetStatMod(string affectedStat)
    {
        statMod StatMod = new statMod(affectedStat, 0, 1, 0);

        foreach(statMod s in modifierList)
        {
            if(s.affectedStat.CompareTo(affectedStat) == 0)
            {
                StatMod.flatMod += s.flatMod;
                StatMod.multMod += s.multMod;
            }
        }

        return StatMod;
    }

    //
    /// <summary>
    /// Some weapons have different effects at different distances from their target
    /// </summary>
    /// <param name="dist">Distance from the target</param>
    /// <returns>The multiplier at that distance, default 1.0</returns>
    private float GetDistMod(int dist)
    {
        float damageMod = 1.0f;
        if (equippedWeapon != null)
        {
            foreach (RangeDependentAttack r in Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[equippedWeapon]).subType].specialRanges)
            {
                if (r.atDistance == dist)
                {
                    damageMod = r.damageMult;
                }
            }
        }
        return damageMod;
    }
    
    //
    //
    //All of these grab the combined total of base stat, weapon stats and stat mods for a certain stat
    //
    //

    public int GetEffectiveMaxHealth()
    {
        int value = mHealth;
        foreach (string i in equipment)
        {
            if (i != null)
            {
                value += ((EquippableBase)Registry.ItemRegistry[i]).health;
            }
        }

        return value;
    }

    public int GetEffectiveAtk(int dist = -1)
    {
        int value = attack;
        foreach(string i in equipment)
        {
            if(i != null)
            {
                if(((EquippableBase)Registry.ItemRegistry[i]).statType == 0)
                    value += ((EquippableBase)Registry.ItemRegistry[i]).strength;
            }
        }
        value = Mathf.RoundToInt(value * GetDistMod(dist));

        statMod s = GetStatMod("atk");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);
        
        return value;
    }

    public int GetEffectiveDef(int dist = -1)
    {
        int value = defense;

        foreach (string i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i]).statType == 0)
                    value += ((EquippableBase)Registry.ItemRegistry[i]).defense;
            }
        }

        statMod s = GetStatMod("def");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetEffectiveMAtk(int dist = -1)
    {
        int value = mAttack;

        foreach (string i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i]).statType == 1)
                    value += ((EquippableBase)Registry.ItemRegistry[i]).strength;
            }
        }

        value = Mathf.RoundToInt(value * GetDistMod(dist));
        statMod s = GetStatMod("matk");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetEffectiveMDef(int dist = -1)
    {
        int value = defense;

        foreach (string i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i]).statType == 1)
                    value += ((EquippableBase)Registry.ItemRegistry[i]).defense;
            }
        }

        statMod s = GetStatMod("mdef");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetEffectiveCrit()
    {
        int value = critChance;
        foreach(string i in equipment)
        {
            if (i != null)
            {
                value += ((EquippableBase)Registry.ItemRegistry[i]).critChanceMod;
            }
        }
        statMod s = GetStatMod("crit");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetMoveSpeed()
    {
        int value = Registry.MovementRegistry[moveType].moveSpeed;
        value += GetStatMod("move").flatMod;
        return value;
    }
    
    /// <summary>
    /// Returns true if sent value matches the code of a tile type this participant can move over
    /// </summary>
    /// <param name="tileValue">Tile type to check for</param>
    public bool ValidMoveTile(int tileValue)
    {
        if (tileValue == 1 || tileValue == 4 || tileValue == 5)
            return true;
        if (tileValue == 2 && Registry.MovementRegistry[moveType].moveOverForest)
            return true;
        if (tileValue == 3 && Registry.MovementRegistry[moveType].moveOverWater)
            return true;
        return false;
    }
    
    /// <summary>
    /// Heals the pawn, making sure it isn't healed past its max health
    /// </summary>
    /// <param name="healAmount">Max amount of health to gain</param>
    public void Heal(int healAmount)
    {
        cHealth = Mathf.Clamp(cHealth + healAmount, 0, GetEffectiveMaxHealth());
    }

    //
    /// <summary>
    /// Deals damage to the pawn, makes sure it isn't overkilled
    /// Also dispells effects that are removed by dealing damage
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public void Damage(int damage)
    {
        cHealth = Mathf.Clamp(cHealth - damage, 0, GetEffectiveMaxHealth());
        if (damage > 0 && statusList.Contains("sleep"))
            statusList.Remove("sleep");
    }
    
    /// <summary>
    /// Iterates through all stat changes and removes any that expire and deals with end of turn status effects
    /// </summary>
    public void EndOfTurn()
    {
        for(int i = 0; i < modifierList.Count; i++)
        {
            modifierList[i].countDown();
            if (modifierList[i].duration == 0)
            {
                modifierList.RemoveAt(i);
                i--;
            }
        }

        //waking up randomly
        if (statusList.Contains("sleep") && Random.Range(1, 4) < 2)
            statusList.Remove("sleep");
    }
}
