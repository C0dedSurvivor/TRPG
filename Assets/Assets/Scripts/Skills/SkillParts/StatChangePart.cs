
public class StatChangePart : SkillPartBase{

    //Stat modifier
    public StatMod StatMod;

    public StatChangePart(TargettingType target, Stats affectedStat, int flatChange, int multiplier, int duration, int chance = 100) : base(target, chance)
    {
        StatMod = new StatMod(affectedStat, flatChange, multiplier, duration);
    }
}
