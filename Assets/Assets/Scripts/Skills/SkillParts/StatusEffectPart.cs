
public class StatusEffectPart : SkillPartBase{

    public string status;
    public bool remove = false;

    public StatusEffectPart(int tT, string statusT, bool removeIt, int chance = 100)
    {
        skillPartType = "statusEffect";
        targetType = tT;
        status = statusT;
        remove = removeIt;
        chanceToProc = chance;
    }
}
