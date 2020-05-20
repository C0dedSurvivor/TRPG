/// <summary>
/// Stores all of the unique, mutable data necessary to correctly active an effect during a battle
/// </summary>
public class TemporaryEffectData
{
    public int usesThisBattle = 0;
    public int turnsSinceLastUse = int.MaxValue;
    public int turnsActive = 0;

    public int maxTimesThisBattle;
    public int turnCooldown;
    public int maxActiveTurns;

    public TemporaryEffectData(int maxTimesThisBattle = -1, int turnCooldown = -1, int maxActiveTurns = -1)
    {
        this.maxTimesThisBattle = maxTimesThisBattle;
        this.turnCooldown = turnCooldown;
        this.maxActiveTurns = maxActiveTurns;
    }

    /// <summary>
    /// If this effect can still be triggered
    /// </summary>
    public bool Activatable()
    {
        //If it has reached its limit of activations per battle
        if (maxTimesThisBattle != -1 && usesThisBattle >= maxTimesThisBattle)
            return false;
        //If it is still on cooldown
        if (turnCooldown != -1 && turnCooldown > turnsSinceLastUse)
            return false;
        return true;
    }

    /// <summary>
    /// Counts up how long it's been since the effect was last triggered at the start of each turn
    /// </summary>
    public void StartOfTurn()
    {
        if (turnsSinceLastUse < int.MaxValue)
            turnsSinceLastUse++;
    }

    /// <summary>
    /// Increments how many turns this effect has been active for
    /// </summary>
    /// <returns>If this effect has run out</returns>
    public bool EndOfTurn()
    {
        if (turnsActive < int.MaxValue)
            turnsActive++;
        return maxActiveTurns != -1 && maxActiveTurns <= turnsActive;
    }

    /// <summary>
    /// Changes relevant values when the skill is activated
    /// </summary>
    public void Activated()
    {
        turnsSinceLastUse = 0;
        usesThisBattle++;
    }

    /// <summary>
    /// Returns whether this effect is out of uses so it can be removed
    /// </summary>
    public bool OutOfUses()
    {
        return maxTimesThisBattle != -1 && maxTimesThisBattle <= usesThisBattle;
    }
}