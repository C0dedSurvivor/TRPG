public class StatusEffectPart : SkillPartBase{

    public string status;
    //Whether this is removing or adding a status effect
    public bool remove = false;

    public StatusEffectPart() { }

    public StatusEffectPart(TargettingType target, string status, bool removeIt, int chance = 100) : base(target, chance)
    {
        this.status = status;
        remove = removeIt;
    }
}
