using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum DamageType
{
    Physical,
    Magical
}

/// <summary>
/// Stores all immutable information for access by other files
/// </summary>
public class Registry
{
    //List of information on all items
    public static IDictionary<string, ItemBase> ItemRegistry = new Dictionary<string, ItemBase>();
    //List of information on all movement types
    public static List<MovementType> MovementRegistry = new List<MovementType>();
    //List of information on all weapon types
    public static List<WeaponType> WeaponTypeRegistry = new List<WeaponType>();
    //List of information on all status effects
    public static IDictionary<string, StatusEffectDefinition> StatusEffectRegistry = new Dictionary<string, StatusEffectDefinition>();
    //List of all of the default effects a tile has.
    public static List<TileType> TileTypeRegistry = new List<TileType>();
    //List of all skill trees
    public static List<SpellTree> SpellTreeRegistry = new List<SpellTree>();
    //Player template definitions
    public static Dictionary<string, EnemyType> PlayerTemplateRegistry = new Dictionary<string, EnemyType>();
    //Enemy template definitions
    public static Dictionary<string, EnemyType> EnemyDefinitionRegistry = new Dictionary<string, EnemyType>();

    public static void FillRegistry()
    {
        using (StreamReader reader = new StreamReader("Definitions.json"))
        {
            var dataDumpType = new
            {
                BaseItems = new ItemBase[] { },
                BattleItems = new BattleItemBase[] { },
                WeaponTypes = new WeaponType[] { },
                Weapons = new EquippableBase[] { },
                Equipment = new EquippableBase[] { },
                StatusEffects = new StatusEffectDefinition[] { },
                SpellTrees = new SpellTree[] { },
                TileEffects = new TileType[] { },
                MoveTypes = new MovementType[] { },
                PlayerDefs = new EnemyType[] { },
                EnemyDefs = new EnemyType[] { }
            };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new SkillPartJsonConverter());

            var dataDump = JsonConvert.DeserializeAnonymousType(reader.ReadToEnd(), dataDumpType, settings);
            foreach (ItemBase item in dataDump.BaseItems) { ItemRegistry.Add(item.name, item); }
            foreach (BattleItemBase item in dataDump.BattleItems) { ItemRegistry.Add(item.name, item); }
            foreach (WeaponType item in dataDump.WeaponTypes) { WeaponTypeRegistry.Add(item); }
            foreach (EquippableBase item in dataDump.Weapons) { ItemRegistry.Add(item.name, item); }
            foreach (EquippableBase item in dataDump.Equipment) { ItemRegistry.Add(item.name, item); }
            foreach (StatusEffectDefinition item in dataDump.StatusEffects) { StatusEffectRegistry.Add(item.name, item); }
            foreach (SpellTree item in dataDump.SpellTrees) { SpellTreeRegistry.Add(item); }
            foreach (TileType item in dataDump.TileEffects) { TileTypeRegistry.Add(item); }
            foreach (MovementType item in dataDump.MoveTypes) { MovementRegistry.Add(item); }
            foreach (EnemyType item in dataDump.PlayerDefs) { PlayerTemplateRegistry.Add(item.name, item); }
            foreach (EnemyType item in dataDump.EnemyDefs) { EnemyDefinitionRegistry.Add(item.name, item); }
        }

        //Adds all the different movement types
        MovementRegistry.Add(new MovementType("Tank", new Dictionary<int, bool>() { {1, false }, {2, true },
            { 4, false }, {8, false }, { 7, false }, {10, false },
            { 9, false }, {6, false } }, 1));
        MovementRegistry.Add(new MovementType("Average", new Dictionary<int, bool>() { {1, false }, {2, true },
            { 4, false }, {8, false }, { 7, false }, {10, false },
            { 9, false }, {6, false } }, 2));
        MovementRegistry.Add(new MovementType("Horse calvalry", new Dictionary<int, bool>() { {1, false }, {2, true },
            { 4, false }, {8, false }, { 7, false }, {10, false },
            { 9, false }, {6, false } }, 3));
        MovementRegistry.Add(new MovementType("Flying calvalry", new Dictionary<int, bool>() { {1, false }, {2, false },
            { 4, false }, {8, false }, { 7, false }, {10, false },
            { 9, false }, {6, false }, {3, false } }, 3));
        MovementRegistry.Add(new MovementType("Water walker", new Dictionary<int, bool>() { {1, false }, {2, true },
            { 4, false }, {8, false }, { 7, false }, {10, false },
            { 9, false }, {6, false }, {3, false } }, 2));

        //Adds all the effects
        StatusEffectRegistry.Add("Sleep", new StatusEffectDefinition("Sleep", CountdownType.None, false, true, 0.25f));
        StatusEffectRegistry.Add("Paralyze", new StatusEffectDefinition("Paralyze", CountdownType.None, true, true));
        /*
        PopulateSkillTree();
        */
    }

    /// <summary>
    /// Populates all of the skill trees with their corresponding skills and adds them to the master list
    /// </summary>
    public static void PopulateSkillTree()
    {
        SpellTree testSkillTree = new SpellTree("Complex Spell Tree");

        //Skill fireball = new Skill("Fireball", 2, 1, 7, 1, 1, 0, 1);
        //fireball.AddDamagePart(2, 5, 5, 0, 0);
        //testSkillTree.Add(1, fireball);

        //Adds all of the skills to the trees

        //test skill to test damage, healing and stat changes
        Skill holyHandGrenade = new Skill("Holy Hand Grenade", TargettingType.AllInRange, 1, 7, 5, 5, 0, 0);
        holyHandGrenade.AddDamagePart(TargettingType.Enemy, DamageType.Magical, 3, 3, 0, 0);
        holyHandGrenade.AddHealPart(TargettingType.Ally, 3, 3, 0, 0);
        holyHandGrenade.AddStatPart(TargettingType.Ally, Stats.Attack, 5, 1, 3);
        testSkillTree.Add(holyHandGrenade);

        Skill firewall = new Skill("Firewall", TargettingType.AllInRange, 4, 7, 1, 3, 1, 1);
        firewall.AddDamagePart(TargettingType.Enemy, DamageType.Magical, 3, 5, 0, 0);
        firewall.AddDependency(1);
        testSkillTree.Add(firewall);

        Skill conflagration = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        conflagration.AddDamagePart(TargettingType.Enemy, DamageType.Magical, 10, 0, 0, 0);
        conflagration.AddStatPart(TargettingType.Enemy, Stats.Attack, 0, 0.75f, 3);
        conflagration.AddDependency(1);
        testSkillTree.Add(conflagration);

        Skill testSkill1 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill1.AddDependency(3);
        testSkillTree.Add(testSkill1);

        Skill testSkill2 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1); //5
        testSkill2.AddDependency(4);
        testSkill2.AddDependency(1);
        testSkillTree.Add(testSkill2);

        Skill testSkill3 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill3.AddDependency(4);
        testSkillTree.Add(testSkill3);

        Skill testSkill4 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill4.AddDependency(5);
        testSkillTree.Add(testSkill4);

        Skill testSkill5 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill5.AddDependency(5);
        testSkillTree.Add(testSkill5);

        Skill testSkill6 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill6.AddDependency(7);
        testSkillTree.Add(testSkill6);

        Skill testSkill7 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1); //10
        testSkill7.AddDependency(7);
        testSkill7.AddDependency(2);
        testSkillTree.Add(testSkill7);

        Skill testSkill8 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill8.AddDependency(7);
        testSkillTree.Add(testSkill8);

        Skill testSkill9 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill9.AddDependency(10);
        testSkillTree.Add(testSkill9);


        SpellTree testSkillTree2 = new SpellTree("Single Tree");
        testSkillTree2.Add(holyHandGrenade);

        SpellTree testSkillTree3 = new SpellTree("Simple Branching Tree");
        testSkillTree3.Add(holyHandGrenade);
        testSkillTree3.Add(firewall);
        testSkillTree3.Add(conflagration);

        SpellTree testSkillTree4 = new SpellTree("Tall Tree");
        testSkillTree4.Add(holyHandGrenade);
        testSkillTree4.Add(firewall);
        testSkillTree4.Add(conflagration);
        Skill testSkill21 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill21.AddDependency(1);
        testSkillTree4.Add(testSkill21);

        Skill testSkill22 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill22.AddDependency(2);
        testSkillTree4.Add(testSkill22);
        Skill testSkill23 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill23.AddDependency(2);
        testSkillTree4.Add(testSkill23);
        Skill testSkill24 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill24.AddDependency(2);
        testSkillTree4.Add(testSkill24);
        Skill testSkill25 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill25.AddDependency(2);
        testSkillTree4.Add(testSkill25);

        Skill testSkill26 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill26.AddDependency(3);
        testSkillTree4.Add(testSkill26);
        Skill testSkill27 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill27.AddDependency(3);
        testSkillTree4.Add(testSkill27);
        Skill testSkill28 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill28.AddDependency(3);
        testSkillTree4.Add(testSkill28);
        Skill testSkill29 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill29.AddDependency(3);
        testSkillTree4.Add(testSkill29);

        Skill testSkill210 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill210.AddDependency(4);
        testSkillTree4.Add(testSkill210);
        Skill testSkill211 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill211.AddDependency(4);
        testSkillTree4.Add(testSkill211);
        Skill testSkill212 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill212.AddDependency(4);
        testSkillTree4.Add(testSkill212);
        Skill testSkill213 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill213.AddDependency(4);
        testSkillTree4.Add(testSkill213);

        //Adds all of the trees to the master list
        SpellTreeRegistry.Add(testSkillTree);
        SpellTreeRegistry.Add(testSkillTree2);
        SpellTreeRegistry.Add(testSkillTree3);
        SpellTreeRegistry.Add(testSkillTree4);
    }
}
