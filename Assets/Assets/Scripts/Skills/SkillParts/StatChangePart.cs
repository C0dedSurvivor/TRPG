
public class StatChangePart : SkillPartBase{

    //Stat modifier
    public statMod StatMod;

    public StatChangePart(TargettingType target, string affectedStat, int flatChange, int multiplier, int duration, int chance = 100) : base(target, chance)
    {
        StatMod = new statMod(affectedStat, flatChange, multiplier, duration);
    }
}
