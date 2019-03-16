
public class DamagePart : SkillPartBase{

    //Raw damage value, affected by enemy defense
    public int damage;
    //Flat damage value
    public int flatDamage;
    //Max health percent damage
    public int maxHpPercent;
    //Remaining health percent damage
    public int remainingHpPercent;
    //How much these values should be changed by an extraneous value, 0 if not at all
    public float modifiedByValue;

    public DamageType damageType;

    /// <summary>
    /// Creates a new damage part with the corresponding damage values
    /// </summary>
    /// <param name="modifiedByValue">If this is affected by a numerical value (EX. dealing damage based on damage taken), how much is it affectd</param>
    public DamagePart(TargettingType target, DamageType damageType, int damage, int flatDamage, int maxHpDamage, int remainingHpDamage, int chance = 100, float modifiedByValue = 0) : base(target, chance)
    {
        this.damageType = damageType;
        this.damage = damage;
        this.flatDamage = flatDamage;
        maxHpPercent = maxHpDamage;
        remainingHpPercent = remainingHpDamage;
        this.modifiedByValue = modifiedByValue;
    }
}
