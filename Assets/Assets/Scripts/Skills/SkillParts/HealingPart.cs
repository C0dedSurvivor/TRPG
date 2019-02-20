
public class HealingPart : SkillPartBase{
    
    //Healing value, affected by your strength
    public int healing;
    //Flat healing value
    public int flatHealing;
    //Max health percent healing
    public int maxHpPercent;

    public HealingPart(int target, int heal, int flatHeal, int maxHPHeal, int chance = 100)
    {
        skillPartType = "healing";
        targetType = target;
        healing = heal;
        flatHealing = flatHeal;
        maxHpPercent = maxHPHeal;
        chanceToProc = chance;
    }
}
