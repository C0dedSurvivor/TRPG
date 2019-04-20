using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class StatMod
{
    //atk, def, matk, mdef, crit, move
    public Stats affectedStat;
    public int flatMod;
    public int multMod;
    public int duration;

    public StatMod(Stats stat, int flat, int mult, int dur)
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