public class StatMod
{
    public Stats affectedStat;
    public int flatChange;
    public float multiplier;
    public int duration;

    public StatMod(Stats stat, int flatChange, float multMod, int dur)
    {
        affectedStat = stat;
        this.flatChange = flatChange;
        multiplier = multMod;
        duration = dur;
    }

    /// <summary>
    /// Called at the end of a turn
    /// </summary>
    public void CountDown()
    {
        duration--;
    }
}