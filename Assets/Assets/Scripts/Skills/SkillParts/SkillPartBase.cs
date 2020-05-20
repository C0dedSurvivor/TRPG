/// <summary>
/// What is a valid target for a given skill
/// </summary>
public enum TargettingType
{
    Self,
    //Care about effect range
    Ally,
    Enemy,
    AllInRange,
    //Don't care about effect range
    AllAllies,
    AllAlliesNotSelf,
    AllEnemies
}

public class SkillPartBase
{
    public TargettingType targetType;

    //1-100
    public int chanceToProc;

    public SkillPartBase() { }

    public SkillPartBase(TargettingType targetType, int chanceToProc)
    {
        this.targetType = targetType;
        this.chanceToProc = chanceToProc;
    }
}
