using System.Collections.Generic;

public enum EquipSlot
{
    Weapon,
    Helmet,
    Chestplate,
    Legs,
    Boots,
    Gloves,
    Accessory
}

/// <summary>
/// An item that can be equipped to a pawn
/// <see cref="BattlePawnBase"/>
/// </summary>
public class EquippableBase : ItemBase
{
    public Dictionary<Stats, int> stats = new Dictionary<Stats, int>();

    public EquipSlot equipSlot;

    /// <summary>
    /// Weapon: ID of it's weapon type
    /// Armor: Heavy, medium, light
    /// </summary>
    public int subType;

    /// <summary>
    /// Keeps track of the battle-mutable effect limiters for each triggerable effect
    /// The TemporaryEffectData here should never be modified
    /// </summary>
    public List<AddTriggerPart> effects = new List<AddTriggerPart>();

    /// <summary>
    /// Get the total raw stats given by equipping this item
    /// </summary>
    public int TotalStats
    {
        get
        {
            int i = 0;
            foreach (Stats stat in stats.Keys)
            {
                i += stats[stat];
            }
            return i;
        }
    }

    public EquippableBase(EquipSlot slot, int subtype, int sellPrice, string flavorText, Dictionary<Stats, int> stats) : base(1, sellPrice, flavorText)
    {
        equipSlot = slot;
        subType = subtype;
        this.stats = stats;
    }

    /// <summary>
    /// Adds a triggered effect to the effect list
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="maxTimesThisBattle"></param>
    /// <param name="turnCooldown"></param>
    /// <param name="maxActiveTurns"></param>
    public void AddEffect(TriggeredEffect effect, int maxTimesThisBattle = -1, int turnCooldown = -1, int maxActiveTurns = -1)
    {
        effects.Add(new AddTriggerPart(TargettingType.Self, effect, maxTimesThisBattle, turnCooldown, maxActiveTurns));
    }
}