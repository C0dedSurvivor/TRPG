using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Physical,
    Magical
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
    public static IDictionary<Statuses, StatusEffectDefinition> StatusEffectRegistry = new Dictionary<Statuses, StatusEffectDefinition>();
    //List of all of the default effects a tile has. First is what happens when a pawn passes over the tile, Second is what happens when they are on it at the end of the turn
    public static IDictionary<int, TileEffects> DefaultTileEffects = new Dictionary<int, TileEffects>();

    public static void FillRegistry()
    {
        //Adds all the different movement types
        MovementRegistry.Add(1, new MovementType("tank", new List<BattleTiles>() { BattleTiles.Normal, BattleTiles.Forest, BattleTiles.Burning, BattleTiles.MoveDown,
            BattleTiles.MoveLeft, BattleTiles.MoveRandom, BattleTiles.MoveRight, BattleTiles.MoveUp }, true, 1));
        MovementRegistry.Add(2, new MovementType("average", new List<BattleTiles>() { BattleTiles.Normal, BattleTiles.Forest, BattleTiles.Burning, BattleTiles.MoveDown,
            BattleTiles.MoveLeft, BattleTiles.MoveRandom, BattleTiles.MoveRight, BattleTiles.MoveUp }, true, 2));
        MovementRegistry.Add(3, new MovementType("horse calvalry", new List<BattleTiles>() { BattleTiles.Normal, BattleTiles.Burning, BattleTiles.MoveDown,
            BattleTiles.MoveLeft, BattleTiles.MoveRandom, BattleTiles.MoveRight, BattleTiles.MoveUp }, true, 3));
        MovementRegistry.Add(4, new MovementType("flying calvalry", new List<BattleTiles>() { BattleTiles.Normal, BattleTiles.Forest, BattleTiles.Burning, BattleTiles.MoveDown,
            BattleTiles.MoveLeft, BattleTiles.MoveRandom, BattleTiles.MoveRight, BattleTiles.MoveUp, BattleTiles.Water }, false, 3));
        MovementRegistry.Add(5, new MovementType("water walker", new List<BattleTiles>() { BattleTiles.Normal, BattleTiles.Forest, BattleTiles.Burning, BattleTiles.MoveDown,
            BattleTiles.MoveLeft, BattleTiles.MoveRandom, BattleTiles.MoveRight, BattleTiles.MoveUp , BattleTiles.Water }, true, 2));

        //Adds all weapon types to registry
        WeaponTypeRegistry.Add(0, new WeaponType("Unarmed"));
        WeaponTypeRegistry[0].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical, false, 0.9f));
        WeaponTypeRegistry.Add(1, new WeaponType("Sword"));
        WeaponTypeRegistry[1].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical));
        WeaponTypeRegistry.Add(2, new WeaponType("Lance"));
        WeaponTypeRegistry[2].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical));
        WeaponTypeRegistry[2].ranges.Add(new WeaponStatsAtRange(2, false, DamageType.Physical, false, 0.85f));
        WeaponTypeRegistry.Add(3, new WeaponType("Axe"));
        WeaponTypeRegistry[3].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical, false, 0.8f));
        WeaponTypeRegistry[3].diagonalRanges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical, false, 0.8f));
        WeaponTypeRegistry.Add(4, new WeaponType("Shield"));
        WeaponTypeRegistry[4].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical));
        WeaponTypeRegistry.Add(5, new WeaponType("Tome"));
        WeaponTypeRegistry[5].ranges.Add(new WeaponStatsAtRange(2, true, DamageType.Magical));
        WeaponTypeRegistry[5].diagonalRanges.Add(new WeaponStatsAtRange(1, true, DamageType.Magical));
        WeaponTypeRegistry.Add(6, new WeaponType("Bow"));
        WeaponTypeRegistry[6].ranges.Add(new WeaponStatsAtRange(2, true, DamageType.Physical));
        WeaponTypeRegistry[6].diagonalRanges.Add(new WeaponStatsAtRange(1, true, DamageType.Physical));
        WeaponTypeRegistry.Add(7, new WeaponType("Healing Staff"));
        WeaponTypeRegistry[7].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical, false, 0.8f));
        WeaponTypeRegistry[7].ranges.Add(new WeaponStatsAtRange(2, true, DamageType.Magical, true, 0.5f));
        WeaponTypeRegistry.Add(8, new WeaponType("Scythe"));
        WeaponTypeRegistry[8].ranges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical, false, 0.9f));
        WeaponTypeRegistry[8].diagonalRanges.Add(new WeaponStatsAtRange(1, false, DamageType.Physical, false, 1.1f));

        //Adds all the weapons
        ItemRegistry.Add("Fists", new EquippableBase(0, 4, 0, "", new Dictionary<Stats, int>()));

        ItemRegistry.Add("Wooden Sword", new EquippableBase(0, 1, 500, "A wooden training sword. Not gonna do a lot of damage, but it can get the job done", new Dictionary<Stats, int>() { { Stats.Attack, 2 } }));
        ItemRegistry.Add("Iron Sword", new EquippableBase(0, 1, 500, "Pretty middle of the road as swords go, but a solid choice nonetheless", new Dictionary<Stats, int>() { { Stats.Attack, 5 } }));
        ItemRegistry.Add("Steel Sword", new EquippableBase(0, 1, 500, "A very well built sword, solid and sharp", new Dictionary<Stats, int>() { { Stats.Attack, 5 }, { Stats.CritChance, 5 } }));
        ItemRegistry.Add("Mirendell", new EquippableBase(0, 1, 500, "A legendary sword. Very shiny", new Dictionary<Stats, int>() { { Stats.Attack, 13 }, { Stats.CritChance, 20 } }));

        ItemRegistry.Add("Wooden Lance", new EquippableBase(0, 2, 500, "A wooden training lance. Surprisingly sharp for what it is", new Dictionary<Stats, int>() { { Stats.Attack, 3 } }));
        ItemRegistry.Add("Iron Lance", new EquippableBase(0, 2, 500, "A sturdy lance. Not the best at piercing armor, but will tear through flesh no problem", new Dictionary<Stats, int>() { { Stats.Attack, 4 } }));
        ItemRegistry.Add("Steel Lance", new EquippableBase(0, 2, 500, "A solid lance that can pierce metal as well as bone", new Dictionary<Stats, int>() { { Stats.Attack, 6 } }));
        ItemRegistry.Add("Sapphire Lance", new EquippableBase(0, 2, 500, "A gorgeous lance that sparkles with every movement. Horrible for stealth but looks more than fabuluous enough to make up for it", new Dictionary<Stats, int>() { { Stats.Attack, 8 }, { Stats.CritChance, 10 } }));
        ItemRegistry.Add("Leviantal", new EquippableBase(0, 2, 500, "A legendary lance", new Dictionary<Stats, int>() { { Stats.Attack, 13 }, { Stats.CritChance, 30 } }));

        ItemRegistry.Add("Wooden Axe", new EquippableBase(0, 3, 500, "A wooden training axe. Not much more than a bludgeoning weapon but it does what it needs to", new Dictionary<Stats, int>() { { Stats.Attack, 3 } }));
        ItemRegistry.Add("Iron Axe", new EquippableBase(0, 3, 500, "As good at cleaving heads as it is wood, this is a good choice for any warrior", new Dictionary<Stats, int>() { { Stats.Attack, 6 } }));
        ItemRegistry.Add("Steel Axe", new EquippableBase(0, 3, 500, "Cuts through metal armor like butter, this is a well crafted axe that can deal massive damage", new Dictionary<Stats, int>() { { Stats.Attack, 9 }, { Stats.CritChance, 15 } }));
        ItemRegistry.Add("Xarok", new EquippableBase(0, 3, 500, "A legendary axe", new Dictionary<Stats, int>() { { Stats.Attack, 12 }, { Stats.CritChance, 35 } }));

        ItemRegistry.Add("Wooden Shield", new EquippableBase(0, 4, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 2 }, { Stats.Defense, 2 } }));
        ItemRegistry.Add("Leather Shield", new EquippableBase(0, 4, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 2 }, { Stats.Defense, 4 } }));
        ItemRegistry.Add("Iron Rimmed Shield", new EquippableBase(0, 4, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 3 }, { Stats.Defense, 6 } }));
        ItemRegistry.Add("Iron Shield", new EquippableBase(0, 4, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 3 }, { Stats.Defense, 8 } }));
        ItemRegistry.Add("Steel Shield", new EquippableBase(0, 4, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 5 }, { Stats.Defense, 20 } }));

        ItemRegistry.Add("A Tome I Guess", new EquippableBase(0, 5, 500, "", new Dictionary<Stats, int>() { { Stats.MagicAttack, 2 } }));
        ItemRegistry.Add("A Better Tome I Guess", new EquippableBase(0, 5, 500, "", new Dictionary<Stats, int>() { { Stats.MagicAttack, 7 } }));
        ItemRegistry.Add("An Even Better Tome I Guess", new EquippableBase(0, 5, 500, "", new Dictionary<Stats, int>() { { Stats.MagicAttack, 10 } }));
        ItemRegistry.Add("The Best Tome I Guess", new EquippableBase(0, 5, 500, "", new Dictionary<Stats, int>() { { Stats.MagicAttack, 13 } }));

        ItemRegistry.Add("Wooden Bow", new EquippableBase(0, 6, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 7 } }));
        ItemRegistry.Add("Iron Bow", new EquippableBase(0, 6, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 9 }, { Stats.CritChance, 10 } }));
        ItemRegistry.Add("Aluminum Bow", new EquippableBase(0, 6, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 11 }, { Stats.CritChance, 20 } }));
        ItemRegistry.Add("Arlia", new EquippableBase(0, 6, 500, "", new Dictionary<Stats, int>() { { Stats.Attack, 14 }, { Stats.CritChance, 20 } }));

        //Adds all the testing items
        ItemRegistry.Add("Animal Tooth", new ItemBase(99, 5, "Might be worth some money"));
        ItemRegistry.Add("Arrows", new ItemBase(100, 4, "For shooting with a bow. Not much more to say here"));
        ItemRegistry.Add("Bandage", new ItemBase(150, 20, "Old fashioned healing, but it does what it needs to"));
        ItemRegistry.Add("Bar of Iron", new ItemBase(30, 100, "A solid bar of iron, used for making heavy weapons. I have no clue how I'm carrying all of this"));
        ItemRegistry.Add("Battle Axe", new EquippableBase(0, 2, 500, "Cleaving skulls", new Dictionary<Stats, int>() { { Stats.Attack, 6 } }));
        ItemRegistry.Add("Bloodstone Necklace", new EquippableBase(6, 1, 1000, "Forged from the blood of my enemies (aka a ruby), this necklace enhances aEtheric spells", 
            new Dictionary<Stats, int>() { { Stats.MaxHealth, 5 }, { Stats.MagicAttack, 2 }, { Stats.SpellDamageEffectiveness, 5 } }));
        ItemRegistry.Add("Copper Ore", new ItemBase(40, 145, "The raw form of copper, an important material for making aEther-based machinery"));
        ItemRegistry.Add("Crossbow", new EquippableBase(0, 6, 600, "Sends a deadly projectile through armor or shadowy... things... all the same", new Dictionary<Stats, int>() { { Stats.Attack, 7 } }));
        ItemRegistry.Add("Dagger", new EquippableBase(0, 0, 500, "A tried and true tool for any job, be it eating, sculpting, or murder", new Dictionary<Stats, int>() { { Stats.Attack, 2 } }));

        ItemRegistry.Add("Egg", new ItemBase(99, 4, "The infinite potential of new life. Or a good breakfast. Your choice"));
        ItemRegistry.Add("Empty Bottle (Large)", new ItemBase(100, 1, "A large bottle of the most dangerous substance in the universe, air. 100% of people exposed to it have died, y'know"));
        ItemRegistry.Add("Feather", new ItemBase(999, 1, "It's a feather. A chicken died for this you monster"));
        ItemRegistry.Add("Magic Dust", new ItemBase(100, 150, "A pile of sand-like substance used in creating aEther-conductive materials and machines"));
        ItemRegistry.Add("Mana Potion (Large)", new ItemBase(100, 150, "A bottle of contained aEther. Can be smashed to restore some aEther to the nearby environment"));
        ItemRegistry.Add("Potion of Healing (Large)", new ItemBase(100, 150, "Heals for 50 health"));
        ItemRegistry.Add("Ruby", new ItemBase(50, 5000, "A beautiful ruby. Can be sold for a high price to shops"));
        ItemRegistry.Add("Staff of Healing", new EquippableBase(0, 7, 1000, "Heals things", new Dictionary<Stats, int>() { { Stats.MaxHealth, 20 }, { Stats.Defense, 5 }, { Stats.MagicAttack, 3 } }));
        
        EquippableBase helmOfHealing = new EquippableBase(1, 0, 100, "It's a helmet. Riding your bike has never been safer!\nHeals for 10 health every other turn.", new Dictionary<Stats, int>() { { Stats.Defense, 5 } });
        helmOfHealing.AddEffect(new TriggeredEffect(EffectTriggers.StartOfTurn, new HealingPart(TargettingType.Self, 0, 10, 0)), -1, 2);
        ItemRegistry.Add("Helmet of Healing", helmOfHealing);

        EquippableBase demonSword = new EquippableBase(0, 0, 500, "This sword has eyes. Why.\nKilling an enemy with a basic attack heals you.", new Dictionary<Stats, int>() { { Stats.Attack, 13 }, { Stats.CritChance, 20 } });
        //If you kill an enemy with a basic attack, heals you for 25% of your health
        demonSword.AddEffect(new TriggeredEffect(EffectTriggers.BasicAttack, new AddTriggerPart(TargettingType.Self, new TriggeredEffect(EffectTriggers.KillAnEnemy, new HealingPart(TargettingType.Self, 0, 0, 25)), 1, -1, 1)));
        ItemRegistry.Add("Demonic Sword", demonSword);

        EquippableBase crescentRose = new EquippableBase(0, 8, 10000, "A breathtaking weapon from another world. It's also a gun!\nKilling an enemy with a basic attack allows you to move again.", 
            new Dictionary<Stats, int>() { { Stats.Attack, 20 }, { Stats.Defense, 10 }, { Stats.MaxMove, 1 } });
        //If you kill an enemy with a basic attack, increase your movement speed by one for this turn and allow you to move again
        crescentRose.AddEffect(new TriggeredEffect(EffectTriggers.BasicAttack, new AddTriggerPart(TargettingType.Self, 
            new TriggeredEffect(EffectTriggers.KillAnEnemy, new StatChangePart(TargettingType.Self, Stats.MaxMove, 1, 1, 1)), 1, -1, 1)));
        crescentRose.AddEffect(new TriggeredEffect(EffectTriggers.BasicAttack, new AddTriggerPart(TargettingType.Self, new 
            TriggeredEffect(EffectTriggers.KillAnEnemy, new UniqueEffectPart(TargettingType.Self, UniqueEffects.MoveAgain)), 1, -1, 1)));
        ItemRegistry.Add("Crescent Rose", crescentRose);

        EquippableBase desperationChestplate = new EquippableBase(2, 0, 500, "A remnant of the desperate last stand of a brave knight.\nMassively increase defense for a short time on low health.", new Dictionary<Stats, int>() { { Stats.MaxHealth, 10 }, { Stats.Defense, 5 } });
        desperationChestplate.AddEffect(new TriggeredEffect(EffectTriggers.FallBelow25Percent, new StatChangePart(TargettingType.Self, Stats.Defense, 20, 1, 2)));
        ItemRegistry.Add("Chestplate of the Last Stand", desperationChestplate);

        EquippableBase amuletOfRejuv = new EquippableBase(6, 0, 500, "An amulet filled with a powerful healing magic.\nGet a burst of healing at low health once per battle.", 
            new Dictionary<Stats, int>() { { Stats.Defense, 3 }, { Stats.MagicDefense, 4 }, { Stats.SpellLifesteal, 5 }, { Stats.HealingEffectiveness, 15 }, { Stats.HealingReceptiveness, 15 } });
        amuletOfRejuv.AddEffect(new TriggeredEffect(EffectTriggers.FallBelow25Percent, new HealingPart(TargettingType.Self, 0, 0, 30)), 1);
        ItemRegistry.Add("Amulet of Rejuvenation", amuletOfRejuv);

        EquippableBase thornyLeggings = new EquippableBase(3, 0, 500, "Leggings possessed by a spirit of vengence.\nReturns some of the damage you take to the attacker.", new Dictionary<Stats, int>() { { Stats.Attack, 5 }, { Stats.Defense, 5 } });
        thornyLeggings.AddEffect(new TriggeredEffect(EffectTriggers.TakeDamage, new DamagePart(TargettingType.Enemy, DamageType.Physical, 1, 0, 0, 0, 100, 0.1f)));
        ItemRegistry.Add("Thorny Leggings", thornyLeggings);

        EquippableBase priestNecklace = new EquippableBase(6, 0, 500, "A holy necklace.\nReturns some of the healing you take to the person that healed you.", new Dictionary<Stats, int>() { { Stats.MaxHealth, 10 }, { Stats.MagicDefense, 3 } });
        priestNecklace.AddEffect(new TriggeredEffect(EffectTriggers.GettingHealed, new HealingPart(TargettingType.Ally, 1, 0, 0, 100, 0.1f)));
        ItemRegistry.Add("Priest Necklace", priestNecklace);

        //Adds all the effects
        StatusEffectRegistry.Add(Statuses.Sleep, new StatusEffectDefinition(CountdownType.None, false, true, 0.25f));
        StatusEffectRegistry.Add(Statuses.Paralyze, new StatusEffectDefinition(CountdownType.None, true, true));

        //Adds all the tile effects
        DefaultTileEffects.Add((int)BattleTiles.Burning, new TileEffects(new Dictionary<MoveTriggers, ExecuteEffectEvent>(){ { MoveTriggers.EndOfTurn, new ExecuteEffectEvent(new DamagePart(TargettingType.AllInRange, DamageType.Physical, 0, 5, 0, 0), null, null) },
            { MoveTriggers.PassOver, new ExecuteEffectEvent(new DamagePart(TargettingType.AllInRange, DamageType.Physical, 0, 2, 0, 0), null, null) } }));
        DefaultTileEffects.Add((int)BattleTiles.MoveDown, new TileEffects(new Dictionary<MoveTriggers, ExecuteEffectEvent>() { { MoveTriggers.EndOfTurn, new ExecuteEffectEvent(new MovePart(TargettingType.AllInRange, MoveDirection.Down, 1, Vector2Int.zero, true), null, null) } }));
        DefaultTileEffects.Add((int)BattleTiles.MoveRight, new TileEffects(new Dictionary<MoveTriggers, ExecuteEffectEvent>() { { MoveTriggers.EndOfTurn, new ExecuteEffectEvent(new MovePart(TargettingType.AllInRange, MoveDirection.Right, 1, Vector2Int.zero, true), null, null) } }));
        DefaultTileEffects.Add((int)BattleTiles.MoveRandom, new TileEffects(new Dictionary<MoveTriggers, ExecuteEffectEvent>() { { MoveTriggers.EndOfTurn, new ExecuteEffectEvent(new MovePart(TargettingType.AllInRange, MoveDirection.Random, 1, Vector2Int.zero, true), null, null) } }));
        DefaultTileEffects.Add((int)BattleTiles.MoveUp, new TileEffects(new Dictionary<MoveTriggers, ExecuteEffectEvent>() { { MoveTriggers.EndOfTurn, new ExecuteEffectEvent(new MovePart(TargettingType.AllInRange, MoveDirection.Up, 1, Vector2Int.zero, true), null, null) } }));
        DefaultTileEffects.Add((int)BattleTiles.MoveLeft, new TileEffects(new Dictionary<MoveTriggers, ExecuteEffectEvent>() { { MoveTriggers.EndOfTurn, new ExecuteEffectEvent(new MovePart(TargettingType.AllInRange, MoveDirection.Left, 1, Vector2Int.zero, true), null, null) } }));
    }
}
