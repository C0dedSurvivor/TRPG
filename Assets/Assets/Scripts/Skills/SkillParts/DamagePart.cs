
public class DamagePart : SkillPartBase{

    //damage value (put through equation)
    public int damage;
    //flat damage value
    public int flatDamage;
    //max health percent damage
    public int mHpDPercent;
    //remaining health percent damage
    public int rHpDPercent;

    public DamagePart(int tT, int d, int fD, int mHpDp, int rHpDp, int chance = 100)
    {
        skillPartType = "damage";
        targetType = tT;
        damage = d;
        flatDamage = fD;
        mHpDPercent = mHpDp;
        rHpDPercent = rHpDp;
        chanceToProc = chance;
    }
}
