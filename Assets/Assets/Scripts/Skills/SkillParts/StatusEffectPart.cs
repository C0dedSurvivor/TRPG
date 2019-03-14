
public class StatusEffectPart : SkillPartBase{

    public string status;
    //Whether this is removing or adding a status effect
    public bool remove = false;

    public StatusEffectPart(TargettingType target, string statusType, bool removeIt, int chance = 100)
    {
        targetType = target;
        status = statusType;
        remove = removeIt;
        chanceToProc = chance;
    }
}
