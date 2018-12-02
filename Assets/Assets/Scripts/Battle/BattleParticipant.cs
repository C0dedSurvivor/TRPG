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

    public string equippedWeapon;

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
        mHealth = 30 + mT;
        cHealth = mHealth;
        equippedWeapon = "Wooden Sword";

        //grab all the skills
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

    public void AddMod(string affectedStat, int flatMod, int multMod, int dur)
    {
        modifierList.Add(new statMod(affectedStat, flatMod, multMod, dur));
    }

    public void AddMod(statMod s)
    {
        modifierList.Add(s);
    }

    public void AddStatusEffect(string status, int dur = -1)
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
            statusList.Add(status, dur);
        }
        else if(statusList.Contains(status) && Registry.StatusEffectRegistry[s].refreshOnDuplication)
        {
            statusList.Refresh(status, dur);
        }

        if (s.CompareTo("sleep") == 0)
            moved = true;
    }

    public void RemoveStatusEffect(string s)
    {
        if (statusList.Contains(s))
        {
            statusList.Remove(s);
            if (s.CompareTo("sleep") == 0)
                moved = false;
        }
    }

    //gets the combined total of all stat mods that affect a single stat on this partcipant
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

    //some weapons get different effects based on distance
    private float GetDistMod(int dist)
    {
        float damageMod = 1.0f;
        foreach (RangeDependentAttack r in Registry.WeaponTypeRegistry[Registry.WeaponRegistry[equippedWeapon].weaponType].specialRanges)
        {
            if (r.atDistance == dist)
            {
                damageMod = r.damageMult;
            }
        }
        return damageMod;
    }

    //all of these grab the combined total of base stat, weapon stats and stat mods for a certain stat
    public int GetEffectiveAtk(int dist = -1)
    {
        int value;
        if (Registry.WeaponTypeRegistry[Registry.WeaponRegistry[equippedWeapon].weaponType].attackType.Contains("magical"))
            value = Mathf.RoundToInt(attack * GetDistMod(dist));
        else
            value = Mathf.RoundToInt((attack + Registry.WeaponRegistry[equippedWeapon].strength) * GetDistMod(dist));
        statMod s = GetStatMod("atk");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);
        
        return value;
    }

    public int GetEffectiveDef(int dist = -1)
    {
        int value;
        if (Registry.WeaponTypeRegistry[Registry.WeaponRegistry[equippedWeapon].weaponType].attackType.Contains("magical"))
            value = Mathf.RoundToInt(defense * GetDistMod(dist));
        else
            value = Mathf.RoundToInt((defense + Registry.WeaponRegistry[equippedWeapon].defense) * GetDistMod(dist));
        statMod s = GetStatMod("def");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetEffectiveMAtk(int dist = -1)
    {
        int value;
        if (Registry.WeaponTypeRegistry[Registry.WeaponRegistry[equippedWeapon].weaponType].attackType.Contains("physical"))
            value = Mathf.RoundToInt(mAttack * GetDistMod(dist));
        else
            value = Mathf.RoundToInt((mAttack + Registry.WeaponRegistry[equippedWeapon].strength) * GetDistMod(dist));
        statMod s = GetStatMod("matk");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetEffectiveMDef(int dist = -1)
    {
        int value;
        if (Registry.WeaponTypeRegistry[Registry.WeaponRegistry[equippedWeapon].weaponType].attackType.Contains("physical"))
            value = Mathf.RoundToInt(mDefense * GetDistMod(dist));
        else
            value = Mathf.RoundToInt((mDefense + Registry.WeaponRegistry[equippedWeapon].defense) * GetDistMod(dist));
        statMod s = GetStatMod("mdef");

        value += s.flatMod;
        value *= (int)(1.0f + 0.125f * s.multMod);

        return value;
    }

    public int GetEffectiveCrit()
    {
        int value;
        value = critChance + Registry.WeaponRegistry[equippedWeapon].critChanceMod;
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

    //Returns true if sent value matches the code of a tile type this participant can move over
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

    //makes sure you can't be overhealed
    public void Heal(int h)
    {
        cHealth = Mathf.Clamp(cHealth + h, 0, mHealth);
    }

    //makes sure you can't be overkilled
    public void Damage(int d)
    {
        cHealth = Mathf.Clamp(cHealth - d, 0, mHealth);
        if (d > 0 && statusList.Contains("sleep"))
            statusList.Remove("sleep");
    }

    //Iterates through all stat changes and removes any that expire and deals with end of turn status effects
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
