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

public enum FacingDirection
{
    North,
    East,
    South,
    West
}

public class BattleParticipant
{
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

    /// <summary>
    /// A list of all temporary triggered effects put on a pawn for a battle
    /// The pair keeps track of the battle-mutable effect limiters for each triggerable effect
    /// First number is the times this was activated this battle
    /// Second number is the amount of turns since this was last used
    /// Third is the number of turns this is valid for and how long it has been since it was added
    /// </summary>
    public List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>> temporaryEffectList = new List<Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>>();

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
        else if (statusList.Contains(status) && Registry.StatusEffectRegistry[s].refreshOnDuplication)
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

        foreach (statMod s in modifierList)
        {
            if (s.affectedStat.CompareTo(affectedStat) == 0)
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
            foreach (RangeDependentAttack r in Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[equippedWeapon.Name]).subType].specialRanges)
            {
                if (r.atDistance == dist)
                {
                    damageMod = r.damageMult;
                }
            }
        }
        return damageMod;
    }


    public List<SkillPartBase> GetTriggeredEffects(EffectTriggers trigger)
    {
        List<SkillPartBase> list = new List<SkillPartBase>();
        foreach(Equippable equipped in equipment)
        {
            if(equipped != null)
                list.AddRange(equipped.GetTriggeredEffects(trigger));
        }

        for (int i = 0; i < temporaryEffectList.Count; i++)
        {
            if (temporaryEffectList[i].First.trigger == trigger)
            {
                //If it has not reached its limit of activations per battle
                if (temporaryEffectList[i].First.maxTimesPerBattle <= 0 || temporaryEffectList[i].Second.First < temporaryEffectList[i].First.maxTimesPerBattle)
                {
                    //If it is not on cooldown
                    if (temporaryEffectList[i].First.delayBetweenUses <= temporaryEffectList[i].Second.Second)
                    {
                        temporaryEffectList[i].Second.First++;
                        temporaryEffectList[i].Second.Second = 0;
                        foreach (SkillPartBase effect in temporaryEffectList[i].First.effects)
                        {
                            list.Add(effect);
                        }
                        //If it has reached its limit of activations per battle, remove it
                        if (temporaryEffectList[i].First.maxTimesPerBattle > 0 && temporaryEffectList[i].Second.First >= temporaryEffectList[i].First.maxTimesPerBattle)
                        {
                            temporaryEffectList.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
        return list;
    }

    //
    //
    //All of these grab the combined total of base stat, weapon stats and stat mods for a certain stat
    //
    //

    public int GetEffectiveMaxHealth()
    {
        int value = mHealth;
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                value += ((EquippableBase)Registry.ItemRegistry[i.Name]).health;
            }
        }

        return value;
    }

    public int GetEffectiveAtk(int dist = -1)
    {
        int value = attack;
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i.Name]).statType == DamageType.Physical)
                    value += ((EquippableBase)Registry.ItemRegistry[i.Name]).strength;
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

        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i.Name]).statType == DamageType.Physical)
                    value += ((EquippableBase)Registry.ItemRegistry[i.Name]).defense;
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

        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i.Name]).statType == DamageType.Magical)
                    value += ((EquippableBase)Registry.ItemRegistry[i.Name]).strength;
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

        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                if (((EquippableBase)Registry.ItemRegistry[i.Name]).statType == DamageType.Magical)
                    value += ((EquippableBase)Registry.ItemRegistry[i.Name]).defense;
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
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                value += ((EquippableBase)Registry.ItemRegistry[i.Name]).critChanceMod;
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
    public int Heal(int healAmount)
    {
        int previous = cHealth;
        cHealth = Mathf.Clamp(cHealth + healAmount, 0, GetEffectiveMaxHealth());
        return cHealth - previous;
    }

    //
    /// <summary>
    /// Deals damage to the pawn, makes sure it isn't overkilled
    /// Also dispells effects that are removed by dealing damage
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public int Damage(int damage)
    {
        if (damage > 0 && statusList.Contains("sleep"))
            statusList.Remove("sleep");
        int previous = cHealth;
        cHealth = Mathf.Clamp(cHealth - damage, 0, GetEffectiveMaxHealth());
        return previous - cHealth;
    }

    public void AddTemporaryTrigger(TriggeredEffect effect, int turnLimit)
    {
        temporaryEffectList.Add(new Pair<TriggeredEffect, Triple<int, int, Pair<int, int>>>(effect, new Triple<int, int, Pair<int, int>>(0, int.MaxValue, new Pair<int, int>(turnLimit, 0))));
    }

    public void StartOfMatch()
    {
        moved = false;
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                i.StartOfMatch();
            }
        }
    }

    public void StartOfTurn()
    {
        foreach (Equippable i in equipment)
        {
            if (i != null)
            {
                i.StartOfTurn();
            }
        }

        for(int i = 0; i < temporaryEffectList.Count; i++)
        {
            if (temporaryEffectList[i].Second.Second < int.MaxValue)
                temporaryEffectList[i].Second.Second++;
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
            modifierList[i].countDown();
            if (modifierList[i].duration == 0)
            {
                modifierList.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < temporaryEffectList.Count; i++)
        {
            if (temporaryEffectList[i].Second.Third.First < int.MaxValue)
                temporaryEffectList[i].Second.Third.First++;
            //If the effect has a turn limit and has reached that turn limit, remove it
            if(temporaryEffectList[i].Second.Third.Second > 0 && temporaryEffectList[i].Second.Third.Second <= temporaryEffectList[i].Second.Third.First)
            {
                temporaryEffectList.RemoveAt(i);
                i--;
            }
        }

        //Waking up randomly
        if (statusList.Contains("sleep") && Random.Range(1, 4) < 2)
            statusList.Remove("sleep");
    }

    /// <summary>
    /// Can the pawn move this turn
    /// </summary>
    public bool CanMove()
    {
        return !statusList.Contains("sleep") && cHealth > 0;
    }
}
