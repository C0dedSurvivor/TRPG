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

    /// <summary>
    /// Returns the name of the status, parsing it if it has a strength modifier at the end
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        if (effect.IndexOf(" ") == -1)
            return effect;
        return effect.Substring(0, effect.IndexOf(" "));
    }

    /// <summary>
    /// Gets the intensity modifier of the status if there is one
    /// EX: "burn 3"
    /// </summary>
    public int GetIntensity()
    {
        if (effect.IndexOf(" ") == -1)
            return 0;
        return int.Parse(effect.Substring(effect.IndexOf(" ") + 1));
    }
}
