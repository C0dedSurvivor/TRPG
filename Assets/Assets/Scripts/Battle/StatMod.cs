using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class StatMod
{
    public Stats affectedStat;
    public int flatMod;
    public float multMod;
    public int duration;

    public StatMod(Stats stat, int flat, float mult, int dur)
    {
        affectedStat = stat;
        flatMod = flat;
        multMod = mult;
        duration = dur;
    }

    public void CountDown()
    {
        duration--;
    }
}