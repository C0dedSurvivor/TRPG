using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Physical,
    Magical
}

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

/// <summary>
/// Some weapons have different stats or effects based on how far away the pawn being hit is
/// </summary>
public struct RangeDependentAttack
{
    //At what distance the wepon gets these different stats
    public int atDistance;
    //Is the weapon ranged. If it is, attack spaces don't depend on whether the previous space is passable
    public bool ranged;
    public DamageType damageType;
    //Damage multiplier
    public float damageMult;
    public RangeDependentAttack(int dist, bool r, DamageType type, float multiplier)
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
    //How far away the weapon's range starts at (i.e. bow range starts 1 block away from the player)
    public int sRange;
    //The range of the weapon starting at the sRange (i.e. lances have a range of 2)
    public int range;
    //The diagonal range of the weapon
    public int diagCut;
    //Is the weapon ranged. If it is, attack spaces don't depend on whether the previous space is passable
    public bool ranged;
    //Does it heal
    public bool heals;
    public DamageType damageType;
    //List of special effects acivated at certain ranges
    public List<RangeDependentAttack> specialRanges;
    public WeaponType(string n, int SRange, int Range, int diagonal, DamageType damageType, bool rangedWeapon, bool canHeal)
    {
        name = n;
        sRange = SRange;
        range = Range;
        diagCut = diagonal;
        this.damageType = damageType;
        ranged = rangedWeapon;
        heals = canHeal;
        specialRanges = new List<RangeDependentAttack>();
    }

    public WeaponType(string n, int SRange, int Range, int diagonal, DamageType damageType, bool rangedWeapon, bool canHeal, RangeDependentAttack rda)
    {
        name = n;
        sRange = SRange;
        range = Range;
        diagCut = diagonal;
        this.damageType = damageType;
        ranged = rangedWeapon;
        heals = canHeal;
        specialRanges = new List<RangeDependentAttack>();
        specialRanges.Add(rda);
    }

    public WeaponType(string n, int SRange, int Range, int diagonal, DamageType damageType, bool rangedWeapon, bool canHeal, List<RangeDependentAttack> rda)
    {
        name = n;
        sRange = SRange;
        range = Range;
        diagCut = diagonal;
        this.damageType = damageType;
        ranged = rangedWeapon;
        heals = canHeal;
        specialRanges = new List<RangeDependentAttack>(rda);
    }
}

/// <summary>
/// Template for an enemy
/// </summary>
public struct EnemyType
{
    string name;

    int level1Atk;
    int level1MAtk;
    int level1Def;
    int level1MDef;
    int level1Health;

    //Average stat growth per level, slightly randomized per level for variation
    int atkGrowth;
    int mAtkGrowth;
    int defGrowth;
    int mDefGrowth;
    int healthGrowth;
}

/// <summary>
/// All of the information pertaining to a specific status effect type
/// </summary>
public struct StatusEffectInfo
{
    //Can multiple of this effect exist on one participant
    public bool stackable;
    //If another copy of this effect is added, refresh the duration instead of having only one or stacking
    public bool refreshOnDuplication;
    //Determines whether a number can be put next to the name to scale the effect
    public bool scalable;
    //Does this effect persist between battles
    public bool persists;

    public StatusEffectInfo(bool stacks, bool refresh, bool scales, bool persistence)
    {
        stackable = stacks;
        refreshOnDuplication = refresh;
        scalable = scales;
        persists = persistence;
    }
}

/// <summary>
/// Stores all immutable information for access by other files
/// </summary>
public class Registry{
    //List of information on all items
    public static IDictionary<string, ItemBase> ItemRegistry = new Dictionary<string, ItemBase>();
    //List of information on all movement types
    public static IDictionary<int, MovementType> MovementRegistry = new Dictionary<int, MovementType>();
    //List of information on all weapon types
    public static IDictionary<int, WeaponType> WeaponTypeRegistry = new Dictionary<int, WeaponType>();
    //List of information on all status effects
    public static IDictionary<string, StatusEffectInfo> StatusEffectRegistry = new Dictionary<string, StatusEffectInfo>();

    public static void FillRegistry()
    {
        //Adds all the different movement types
        MovementRegistry.Add(1, new MovementType("tank", false, true, true, 1));
        MovementRegistry.Add(2, new MovementType("average", false, true, true, 2));
        MovementRegistry.Add(3, new MovementType("horse calvalry", false, false, true, 3));
        MovementRegistry.Add(4, new MovementType("flying calvalry", true, true, false, 3));
        MovementRegistry.Add(5, new MovementType("water walker", true, true, true, 2));

        //Adds all weapon types to registry
        WeaponTypeRegistry.Add(0, new WeaponType("Sword", 0, 1, 0, DamageType.Physical, false, false));
        RangeDependentAttack rda = new RangeDependentAttack(2, false, 0, 0.85f);
        WeaponTypeRegistry.Add(1, new WeaponType("Lance", 0, 2, 0, DamageType.Physical, false, false, rda));
        WeaponTypeRegistry.Add(2, new WeaponType("Axe", 0, 1, 1, DamageType.Physical, false, false));
        WeaponTypeRegistry.Add(3, new WeaponType("Unarmed", 0, 1, 0, DamageType.Physical, false, false));
        WeaponTypeRegistry.Add(4, new WeaponType("Shield", 0, 1, 0, DamageType.Physical, false, false));
        WeaponTypeRegistry.Add(5, new WeaponType("Tome", 1, 1, 1, DamageType.Magical, true, false));
        WeaponTypeRegistry.Add(6, new WeaponType("Bow", 1, 1, 1, DamageType.Physical, true, false));
        WeaponTypeRegistry.Add(7, new WeaponType("Healing Staff", 0, 2, 1, DamageType.Magical, true, true));

        //Adds all the weapons
        ItemRegistry.Add("Wooden Sword", new EquippableBase(0, 0, DamageType.Physical, 0, 2, 0, 0, 500, "A wooden training sword. Not gonna do a lot of damage, but it can get the job done"));
        ItemRegistry.Add("Iron Sword", new EquippableBase(0, 0, DamageType.Physical, 0, 5, 0, 0, 500, "Pretty middle of the road as swords go, but a solid choice nonetheless"));
        ItemRegistry.Add("Steel Sword", new EquippableBase(0, 0, DamageType.Physical, 0, 8, 0, 5, 500, "A very well built sword, solid and sharp"));
        ItemRegistry.Add("Mirendell", new EquippableBase(0, 0, DamageType.Physical, 0, 13, 0, 20, 500, "A legendary sword. Very shiny"));

        ItemRegistry.Add("Wooden Lance", new EquippableBase(0, 1, DamageType.Physical, 0, 3, 0, 0, 500, "A wooden training lance. Surprisingly sharp for what it is"));
        ItemRegistry.Add("Iron Lance", new EquippableBase(0, 1, DamageType.Physical, 0, 4, 0, 0, 500, "A sturdy lance. Not the best at piercing armor, but will tear through flesh no problem"));
        ItemRegistry.Add("Steel Lance", new EquippableBase(0, 1, DamageType.Physical, 0, 6, 0, 0, 500, "A solid lance that can pierce metal as well as bone"));
        ItemRegistry.Add("Sapphire Lance", new EquippableBase(0, 1, DamageType.Physical, 0, 8, 0, 10, 500, "A gorgeous lance that sparkles with every movement. Horrible for stealth but looks more than fabuluous enough to make up for it"));
        ItemRegistry.Add("Leviantal", new EquippableBase(0, 1, DamageType.Physical, 0, 13, 0, 30, 500, "A legendary lance"));

        ItemRegistry.Add("Wooden Axe", new EquippableBase(0, 2, DamageType.Physical, 0, 3, 0, 0, 500, "A wooden training axe. Not much more than a bludgeoning weapon but whatever"));
        ItemRegistry.Add("Iron Axe", new EquippableBase(0, 2, DamageType.Physical, 0, 6, 0, 0, 500, "As good at cleaving heads as it is wood, this is a good choice for any warrior"));
        ItemRegistry.Add("Steel Axe", new EquippableBase(0, 2, DamageType.Physical, 0, 9, 0, 15, 500, "Cuts through metal armor like butter, this is a well crafted axe that can deal massive damage"));
        ItemRegistry.Add("Xarok", new EquippableBase(0, 2, DamageType.Physical, 0, 12, 0, 35, 500, "A legendary axe"));

        ItemRegistry.Add("Fists", new EquippableBase(0, 3, DamageType.Physical, 0, 2, 1, 0, 500));

        ItemRegistry.Add("Wooden Shield", new EquippableBase(0, 4, DamageType.Physical, 0, 2, 2, 0, 500));
        ItemRegistry.Add("Leather Shield", new EquippableBase(0, 4, DamageType.Physical, 0, 2, 4, 0, 500));
        ItemRegistry.Add("Iron Rimmed Shield", new EquippableBase(0, 4, DamageType.Physical, 0, 3, 6, 0, 500));
        ItemRegistry.Add("Iron Shield", new EquippableBase(0, 4, DamageType.Physical, 0, 3, 8, 0, 500));
        ItemRegistry.Add("Steel Shield", new EquippableBase(0, 4, DamageType.Physical, 0, 5, 10, 0, 500));

        ItemRegistry.Add("A Tome I Guess", new EquippableBase(0, 5, DamageType.Magical, 0, 5, 0, 0, 500));
        ItemRegistry.Add("A Better Tome I Guess", new EquippableBase(0, 5, DamageType.Magical, 0, 7, 0, 0, 500));
        ItemRegistry.Add("An Even Better Tome I Guess", new EquippableBase(0, 5, DamageType.Magical, 0, 10, 0, 0, 500));
        ItemRegistry.Add("The Best Tome I Guess", new EquippableBase(0, 5, DamageType.Magical, 0, 13, 0, 0, 500));

        ItemRegistry.Add("Wooden Bow", new EquippableBase(0, 6, DamageType.Physical, 0, 7, 0, 0, 500));
        ItemRegistry.Add("Iron Bow", new EquippableBase(0, 6, DamageType.Physical, 0, 9, 0, 10, 500));
        ItemRegistry.Add("Aluminum Bow", new EquippableBase(0, 6, DamageType.Physical, 0, 11, 0, 20, 500));
        ItemRegistry.Add("Arlia", new EquippableBase(0, 6, DamageType.Physical, 0, 14, 0, 20, 500));

        //Adds all the testing items
        ItemRegistry.Add("Animal Tooth", new ItemBase(99, 5, "Might be worth some money"));
        ItemRegistry.Add("Arrows", new ItemBase(100, 4, "For shooting with a bow. Not much more to say here"));
        ItemRegistry.Add("Bandage", new ItemBase(150, 20, "Old fashioned healing, but it does what it needs to"));
        ItemRegistry.Add("Bar of Iron", new ItemBase(30, 100, "A solid bar of iron, used for making heavy weapons. I have no clue how I'm carrying all of this"));
        ItemRegistry.Add("Battle Axe", new EquippableBase(0, 2, DamageType.Physical, 0, 6, 0, 0, 500, "Cleaving skulls"));
        ItemRegistry.Add("Bloodstone Necklace", new EquippableBase(6, 1, DamageType.Magical, 5, 2, 0, 0, 1000, "Forged from the blood of my enemies (aka a ruby), this necklace enhances aEtheric spells"));
        ItemRegistry.Add("Copper Ore", new ItemBase(40, 145, "The raw form of copper, an important material for making aEther-based machinery"));
        ItemRegistry.Add("Crossbow", new EquippableBase(0, 6, DamageType.Physical, 0, 7, 0, 0, 600, "Sends a deadly projectile through armor or shadowy... things... all the same"));
        ItemRegistry.Add("Dagger", new EquippableBase(0, 0, DamageType.Physical, 0, 2, 0, 0, 500, "A tried and true tool for any job, be it eating, sculpting, or murder"));
        ItemRegistry.Add("Demonic Sword", new EquippableBase(0, 0, DamageType.Physical, 0, 13, 0, 20, 500, "This sword has eyes. Why"));
        ItemRegistry.Add("Egg", new ItemBase(99, 4, "The infinite potential of new life. Or a good breakfast. Your choice"));
        ItemRegistry.Add("Empty Bottle (Large)", new ItemBase(100, 1, "A large bottle of the most dangerous substance, air. 100% of people exposed to it have died, y'know"));
        ItemRegistry.Add("Feather", new ItemBase(999, 1, "It's a feather. A chicken died this you monster"));
        //Testing out passive effects
        EquippableBase test = new EquippableBase(1, 0, DamageType.Physical, 0, 0, 5, 0, 100, "It's a helmet. Riding your bike has never been safer!");
        test.AddEffect(new TriggeredEffect(EffectTriggers.StartOfTurn, 1, 2, new HealingPart(TargettingType.Self, 0, 10, 0)));
        ItemRegistry.Add("Helmet", test);
        ItemRegistry.Add("Magic Dust", new ItemBase(100, 150, "A pile of sand-like substance used in creating aEther-conductive materials and machines"));
        ItemRegistry.Add("Mana Potion (Large)", new ItemBase(100, 150, "A bottle of contained aEther. Can be smashed to restore some aEther to the nearby environment"));
        ItemRegistry.Add("Potion of Healing (Large)", new ItemBase(100, 150, "Heals for 50 health"));
        ItemRegistry.Add("Ruby", new ItemBase(50, 5000, "A beautiful ruby. Can be sold for a high price to shops"));
        ItemRegistry.Add("Staff of Healing", new EquippableBase(0, 7, DamageType.Magical, 20, 0, 5, 0, 1000, "Heals things"));

        //Adds all the effects
        StatusEffectRegistry.Add("sleep", new StatusEffectInfo(false, false, false, false));
        StatusEffectRegistry.Add("DoT", new StatusEffectInfo(true, false, true, false));
        StatusEffectRegistry.Add("paralysis", new StatusEffectInfo(false, false, false, false));
        StatusEffectRegistry.Add("HoT", new StatusEffectInfo(true, false, true, false));
    }
}
