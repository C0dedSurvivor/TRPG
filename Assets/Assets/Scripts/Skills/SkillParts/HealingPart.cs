public class HealingPart : SkillPartBase
{
    //Healing value, affected by your strength
    public int healing;
    //Flat healing value
    public int flatHealing;
    //Max health percent healing
    public int maxHpPercent;
    //How much these values should be changed by an extraneous value, 0 if not at all
    public float modifiedByValue;

    public HealingPart() { }

    /// <summary>
    /// Creates a new damage part with the corresponding damage values
    /// </summary>
    /// <param name="modifiedByValue">If this is affected by a numerical value (EX. healing based on healing recieved), how much is it affectd</param>
    public HealingPart(TargettingType target, int heal, int flatHeal, int maxHPHeal, int chance = 100, float modifiedByValue = 0) : base(target, chance)
    {
        healing = heal;
        flatHealing = flatHeal;
        maxHpPercent = maxHPHeal;
        this.modifiedByValue = modifiedByValue;
    }
}
