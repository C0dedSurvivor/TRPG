
public class HealingPart : SkillPartBase{
    
    //healing value (put through equation)
    public int healing;
    //flat healing value
    public int flatHealing;
    //max health percent healing
    public int mHpHPercent;

    public HealingPart(int tT, int h, int fH, int mHpHp, int chance = 100)
    {
        skillPartType = "healing";
        targetType = tT;
        healing = h;
        flatHealing = fH;
        mHpHPercent = mHpHp;
        chanceToProc = chance;
    }
}
