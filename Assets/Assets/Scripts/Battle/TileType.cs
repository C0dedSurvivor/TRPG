/// <summary>
/// What events can trigger a tile effect
/// </summary>
public enum MoveTriggers
{
    StartOfTurn,
    PassOver,
    StopOnTile,
    EndOfTurn
}

public class TileType
{
    public string name { get; set; }
    public string flavorText;
    //If melee attacks can happen when this tile is between the attacker and target
    public bool blocksMeleeAttacks;
    //If ranged attacks can happen when this tile is between the attacker and target
    public bool blocksRangedAttacks;
    public SkillPartBase startOfTurn;
    public SkillPartBase passOver;
    public SkillPartBase stopOnTile;
    public SkillPartBase endOfTurn;

    /// <summary>
    /// Gets what effect should trigger when an event happens on this tile type
    /// </summary>
    /// <param name="trigger">The event that happened on the tile</param>
    /// <returns>The effect that triggers</returns>
    public ExecuteEffectEvent this[MoveTriggers trigger]
    {
        get
        {
            switch (trigger)
            {
                case MoveTriggers.StartOfTurn: return new ExecuteEffectEvent(startOfTurn, null, null);
                case MoveTriggers.PassOver: return new ExecuteEffectEvent(passOver, null, null);
                case MoveTriggers.StopOnTile: return new ExecuteEffectEvent(stopOnTile, null, null);
                case MoveTriggers.EndOfTurn: return new ExecuteEffectEvent(endOfTurn, null, null);
            }
            return null;
        }
    }

    public TileType(string name, string flavorText, bool blocksMelee = false, bool blocksRanged = false, SkillPartBase startOfTurn = null,
        SkillPartBase passOver = null, SkillPartBase stopOnTile = null, SkillPartBase endOfTurn = null)
    {
        this.name = name;
        this.flavorText = flavorText;
        blocksMeleeAttacks = blocksMelee;
        blocksRangedAttacks = blocksRanged;
        this.startOfTurn = startOfTurn;
        this.passOver = passOver;
        this.stopOnTile = stopOnTile;
        this.endOfTurn = endOfTurn;
    }

    /// <summary>
    /// Does this tile type have an effect that triggers at the given event happening
    /// </summary>
    /// <param name="trigger">The event that happens on the tile</param>
    /// <returns>If an effect gets triggered</returns>
    public bool Contains(MoveTriggers trigger)
    {
        return trigger == MoveTriggers.StartOfTurn && startOfTurn != null ||
            trigger == MoveTriggers.PassOver && passOver != null ||
            trigger == MoveTriggers.StopOnTile && stopOnTile != null ||
            trigger == MoveTriggers.EndOfTurn && endOfTurn != null;
    }
}