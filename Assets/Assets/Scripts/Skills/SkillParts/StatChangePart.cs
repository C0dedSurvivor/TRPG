
public class StatChangePart : SkillPartBase{

    //Stat modifier
    public statMod StatMod;

    public StatChangePart(int target, string affectedStat, int flatChange, int multiplier, int duration, int chance = 100)
    {
        skillPartType = "statChange";
        targetType = target;
        StatMod = new statMod(affectedStat, flatChange, multiplier, duration);
        chanceToProc = chance;
    }
}
