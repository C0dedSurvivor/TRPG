using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectList
{
    public List<StatusEffect> effectList;

    public StatusEffectList()
    {
        effectList = new List<StatusEffect>();
    }

    /// <summary>
    /// Returns if the list contains a certain status condition
    /// </summary>
    /// <param name="status">The status effect to check for</param>
    public bool Contains(string status)
    {
        foreach (StatusEffect se in effectList)
        {
            if (se.effect.IndexOf(status) != -1)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Updates an existing status effect to a new duration
    /// </summary>
    /// <param name="status"></param>
    /// <param name="duration"></param>
    public void Refresh(string status, int duration)
    {
        for(int i = 0; i < effectList.Count; i++)
        {
            if (effectList[i].effect.CompareTo(status) == 0)
                effectList[i].duration = duration;
        }
    }

    /// <summary>
    /// Adds a new status effect to the list
    /// </summary>
    /// <param name="status">The status to add</param>
    /// <param name="duration">The duration of the status, -1 if not removed by time</param>
    public void Add(string status, int duration = -1)
    {
        effectList.Add(new StatusEffect(status, duration));
    }

    /// <summary>
    /// Removes a given status from the list
    /// </summary>
    /// <param name="status">Status effect to remove</param>
    public void Remove(string status)
    {
        for(int i = 0; i < effectList.Count; i++)
        {
            if (effectList[i].effect.IndexOf(status) != -1)
                effectList.RemoveAt(i);
        }
    }
}