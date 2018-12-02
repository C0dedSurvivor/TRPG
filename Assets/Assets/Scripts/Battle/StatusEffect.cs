using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public string effect;
    public int duration;

    public StatusEffect(string status, int dur)
    {
        effect = status;
        duration = dur;
    }

    public string GetName()
    {
        if (effect.IndexOf(" ") == -1)
            return effect;
        return effect.Substring(0, effect.IndexOf(" "));
    }

    public int GetIntensity()
    {
        if (effect.IndexOf(" ") == -1)
            return 0;
        return int.Parse(effect.Substring(effect.IndexOf(" ") + 1));
    }
}
