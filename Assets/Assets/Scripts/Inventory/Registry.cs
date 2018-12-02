using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MovementType
{
    public string name;
    public bool moveOverWater;
    public bool moveOverForest;
    public bool hinderedByForest;
    public int moveSpeed;

    public MovementType(string n, bool mOW, bool mOF, bool hBF, int mS)
    {
        name = n;
        moveOverWater = mOW;
        moveOverForest = mOF;
        hinderedByForest = hBF;
        moveSpeed = mS;
    }
}

//some weapons have different stats or effects based on how far away the enemy being hit is
public struct RangeDependentAttack
{
    //At what distance the wepon gets these different stats
    public int atDistance;
    //Is the weapon ranged. If it is, attack spaces don't depend on whether the previous space is passable
    public bool ranged;
    //Whether it's damage is counted as magic or physical
    public string damageType;
    //Damage multiplier
    public float damageMult;
    public RangeDependentAttack(int dist, bool r, string type, float multiplier)
    {
        atDistance = dist;
        ranged = r;
        damageType = type;
        damageMult = multiplier;
    }
}

public struct WeaponType
{
    string name;
    //how far away the weapon's range starts at (i.e. bow range starts 1 block away from the player)
    public int sRange;
    //the range of the weapon starting at the sRange (i.e. lances have a range of 2)
    public int range;
    //the diagonal range of the weapon
    public int diagCut;
    //Is the weapon ranged. If it is, attack spaces don't depend on whether the previous space is passable
    public bool ranged;
    //Whether it's damage is counted as magic, physical, healing, magic and healing or physical and healing
    public string attackType;
    //List of special effects acivated at certain ranges
    public List<RangeDependentAttack> specialRanges;
    public WeaponType(string n, int sR, int R, int dC, bool r, string dT)
    {
        name = n;
        sRange = sR;
        range = R;
        diagCut = dC;
        ranged = r;
        attackType = dT;
        specialRanges = new List<RangeDependentAttack>();
    }

    public WeaponType(string n, int sR, int R, int dC, bool r, string dT, RangeDependentAttack rda)
    {
        name = n;
        sRange = sR;
        range = R;
        diagCut = dC;
        ranged = r;
        attackType = dT;
        specialRanges = new List<RangeDependentAttack>();
        specialRanges.Add(rda);
    }

    public WeaponType(string n, int sR, int R, int dC, bool r, string dT, List<RangeDependentAttack> rda)
    {
        name = n;
        sRange = sR;
        range = R;
        diagCut = dC;
        ranged = r;
        attackType = dT;
        specialRanges = new List<RangeDependentAttack>(rda);
    }
}

public struct enemyType
{
    string name;

    int level1Atk;
    int level1MAtk;
    int level1Def;
    int level1MDef;
    int level1Health;

    //average stat growth per level, slightly randomized per level for variation
    int atkGrowth;
    int mAtkGrowth;
    int defGrowth;
    int mDefGrowth;
    int healthGrowth;
}

public struct statusEffectInfo
{
    //can multiple of this effect exist on one participant
    public bool stackable;
    //if another copy of this effect is added, refresh the duration instead of having only one or stacking
    public bool refreshOnDuplication;
    //determines whether a number can be put next to the name to scale the effect
    public bool scalable;
    //does this effect persist between battles
    public bool persists;

    public statusEffectInfo(bool stacks, bool refresh, bool scales, bool persistence)
    {
        stackable = stacks;
        refreshOnDuplication = refresh;
        scalable = scales;
        persists = persistence;
    }
}

public class Registry{

    public static IDictionary<string, Weapon> WeaponRegistry = new Dictionary<string, Weapon>();
    public static IDictionary<int, MovementType> MovementRegistry = new Dictionary<int, MovementType>();
    public static IDictionary<int, WeaponType> WeaponTypeRegistry = new Dictionary<int, WeaponType>();
    public static IDictionary<string, statusEffectInfo> StatusEffectRegistry = new Dictionary<string, statusEffectInfo>();

    public static void FillRegistry()
    {
        //adds all the different movement types
        MovementRegistry.Add(1, new MovementType("tank", false, true, true, 1));
        MovementRegistry.Add(2, new MovementType("average", false, true, true, 2));
        MovementRegistry.Add(3, new MovementType("horse calvalry", false, false, true, 3));
        MovementRegistry.Add(4, new MovementType("flying calvalry", true, true, false, 3));
        MovementRegistry.Add(5, new MovementType("water walker", true, true, true, 2));

        //adds all weapon types to registry
        WeaponTypeRegistry.Add(0, new WeaponType("Sword", 0, 1, 0, false, "physical"));
        RangeDependentAttack rda = new RangeDependentAttack(2, false, "physical", 0.85f);
        WeaponTypeRegistry.Add(1, new WeaponType("Lance", 0, 2, 0, false, "physical", rda));
        WeaponTypeRegistry.Add(2, new WeaponType("Axe", 0, 1, 1, false, "physical"));
        WeaponTypeRegistry.Add(3, new WeaponType("Unarmed", 0, 1, 0, false, "physical"));
        WeaponTypeRegistry.Add(4, new WeaponType("Shield", 0, 1, 0, false, "physical"));
        WeaponTypeRegistry.Add(5, new WeaponType("Tome", 1, 1, 1, true, "magical"));
        WeaponTypeRegistry.Add(6, new WeaponType("Bow", 1, 1, 1, true, "physical"));

        //adds all the weapons
        WeaponRegistry.Add("Wooden Sword", new Weapon(0, 2, 0, 0));
        WeaponRegistry.Add("Iron Sword", new Weapon(0, 5, 0, 0));
        WeaponRegistry.Add("Steel Sword", new Weapon(0, 8, 0, 5));
        WeaponRegistry.Add("Mirendell", new Weapon(0, 13, 0, 20));

        WeaponRegistry.Add("Wooden Lance", new Weapon(1, 3, 0, 0));
        WeaponRegistry.Add("Iron Lance", new Weapon(1, 4, 0, 0));
        WeaponRegistry.Add("Steel Lance", new Weapon(1, 6, 0, 0));
        WeaponRegistry.Add("Sapphire Lance", new Weapon(1, 8, 0, 10));
        WeaponRegistry.Add("Leviantal", new Weapon(1, 13, 0, 30));

        WeaponRegistry.Add("Wooden Axe", new Weapon(2, 3, 0, 0));
        WeaponRegistry.Add("Iron Axe", new Weapon(2, 6, 0, 0));
        WeaponRegistry.Add("Steel Axe", new Weapon(2, 9, 0, 15));
        WeaponRegistry.Add("Xarok", new Weapon(2, 12, 0, 35));

        WeaponRegistry.Add("Fists", new Weapon(3, 2, 1, 0));

        WeaponRegistry.Add("Wooden Shield", new Weapon(4, 2, 2, 0));
        WeaponRegistry.Add("Leather Shield", new Weapon(4, 2, 4, 0));
        WeaponRegistry.Add("Iron Rimmed Shield", new Weapon(4, 3, 6, 0));
        WeaponRegistry.Add("Iron Shield", new Weapon(4, 3, 8, 0));
        WeaponRegistry.Add("Steel Shield", new Weapon(4, 5, 10, 0));

        WeaponRegistry.Add("A Tome I Guess", new Weapon(5, 5, 0, 0));
        WeaponRegistry.Add("A Better Tome I Guess", new Weapon(5, 7, 0, 0));
        WeaponRegistry.Add("An Even Better Tome I Guess", new Weapon(5, 10, 0, 0));
        WeaponRegistry.Add("The Best Tome I Guess", new Weapon(5, 13, 0, 0));

        WeaponRegistry.Add("Wooden Bow", new Weapon(6, 7, 0, 0));
        WeaponRegistry.Add("Iron Bow", new Weapon(6, 9, 0, 10));
        WeaponRegistry.Add("Aluminum Bow", new Weapon(6, 11, 0, 20));
        WeaponRegistry.Add("Arlia", new Weapon(6, 14, 0, 20));

        //adds all the effects
        StatusEffectRegistry.Add("sleep", new statusEffectInfo(false, false, false, false));
        StatusEffectRegistry.Add("DoT", new statusEffectInfo(true, false, true, false));
        StatusEffectRegistry.Add("paralysis", new statusEffectInfo(false, false, false, false));
        StatusEffectRegistry.Add("HoT", new statusEffectInfo(true, false, true, false));
    }
}
