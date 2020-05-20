
public class StatChangePart : SkillPartBase
{
    //Stat modifier
    public StatMod statMod;

    public StatChangePart() { }

    public StatChangePart(TargettingType target, Stats affectedStat, int flatChange, float multiplier, int duration, int chance = 100) : base(target, chance)
    {
        statMod = new StatMod(affectedStat, flatChange, multiplier, duration);
    }
}
