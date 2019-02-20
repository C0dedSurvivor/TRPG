
public class StatusEffectPart : SkillPartBase{

    public string status;
    //Whether this is removing or adding a status effect
    public bool remove = false;

    public StatusEffectPart(int target, string statusType, bool removeIt, int chance = 100)
    {
        skillPartType = "statusEffect";
        targetType = target;
        status = statusType;
        remove = removeIt;
        chanceToProc = chance;
    }
}
