public enum Statuses
{
    Stun,
    Paralyze,
    Burn,
    Acid,
    Sleep,
    //Should only be called for removal 
    All
}

public class StatusEffectPart : SkillPartBase{

    public Statuses status;
    //Whether this is removing or adding a status effect
    public bool remove = false;

    public StatusEffectPart(TargettingType target, Statuses status, bool removeIt, int chance = 100) : base(target, chance)
    {
        this.status = status;
        remove = removeIt;
    }
}
