
public class HealingPart : SkillPartBase{
    
    //Healing value, affected by your strength
    public int healing;
    //Flat healing value
    public int flatHealing;
    //Max health percent healing
    public int maxHpPercent;

    public HealingPart(TargettingType target, int heal, int flatHeal, int maxHPHeal, int chance = 100)
    {
        targetType = target;
        healing = heal;
        flatHealing = flatHeal;
        maxHpPercent = maxHPHeal;
        chanceToProc = chance;
    }
}
