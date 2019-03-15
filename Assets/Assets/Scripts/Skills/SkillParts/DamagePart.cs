
public class DamagePart : SkillPartBase{

    //Raw damage value, affected by enemy defense
    public int damage;
    //Flat damage value
    public int flatDamage;
    //Max health percent damage
    public int maxHpPercent;
    //Remaining health percent damage
    public int remainingHpPercent;

    public DamageType damageType;

    public DamagePart(TargettingType target, DamageType damageType, int damage, int flatDamage, int maxHpDamage, int remainingHpDamage, int chance = 100)
    {
        targetType = target;
        this.damageType = damageType;
        this.damage = damage;
        this.flatDamage = flatDamage;
        maxHpPercent = maxHpDamage;
        remainingHpPercent = remainingHpDamage;
        chanceToProc = chance;
    }
}
