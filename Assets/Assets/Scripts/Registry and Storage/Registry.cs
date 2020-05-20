using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

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

    /// <summary>
    /// Loads in the registry values from the path given in StorageDirectory
    /// </summary>
    public static void FillRegistry()
    {
        using (StreamReader reader = new StreamReader(StorageDirectory.DefintionsPath))
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
    }
}
