
public class StatChangePart : SkillPartBase{

    //stat modifier
    public statMod StatMod;

    public StatChangePart(int tT, string affectedStat, int flat, int multiplier, int dur, int chance = 100)
    {
        skillPartType = "statChange";
        targetType = tT;
        StatMod = new statMod(affectedStat, flat, multiplier, dur);
        chanceToProc = chance;
    }
}
