using System;
using System.Collections;
using System.Collections.Generic;

public class Skill {
    public string name;

    //1 = self, 2 = enemy, 3 = ally, 4 = passive, 5 = anywhere
    public int targetType;

    public int aEtherCost;

    public int unlockLevel;
    public int unlockCost;

    //how large the aoe is (even numbers put the extra space on bottom)
    public int xRange;
    public int yRange;

    public int targettingRange;

    //What needs to be unlocked before this one can be, n = id of dependency
    public List<int> dependencies = new List<int>();
    //list of skill parts in order of execution
    public List<SkillPartBase> partList = new List<SkillPartBase>();

    public Skill(string n, int tT, int cost, int targetRange, int xR, int yR, int unlockC, int unlockLvl)
    {
        name = n;
        targetType = tT;
        aEtherCost = cost;
        targettingRange = targetRange;
        xRange = xR;
        yRange = yR;
        unlockCost = unlockC;
        unlockLevel = unlockLvl;
    }

    //adds the ID of a skill that needs to be unlocked before this one becomes unlockable
    public void addDependency(int d)
    {
        dependencies.Add(d);
    }

    public void addDamagePart(int target, int damage, int flatDamage, int percentMaxHealth, int percentCurrentHealth)
    {
        partList.Add(new DamagePart(target, damage, flatDamage, percentMaxHealth, percentCurrentHealth));
    }

    public void addHealPart(int target, int healing, int flatHealing, int percentMaxHealth, int percentCurrentHealth)
    {
        partList.Add(new HealingPart(target, healing, flatHealing, percentMaxHealth, percentCurrentHealth));
    }

    public void addStatPart(int target, string affectedStat, int flat, int multiplier, int dur, int chance = 100)
    {
        partList.Add(new StatChangePart(target, affectedStat, flat, multiplier, dur, chance));
    }

    public void addStatusPart(int target, string status, bool remove, int chance)
    {
        partList.Add(new StatusEffectPart(target, status, remove, chance));
    }
}
