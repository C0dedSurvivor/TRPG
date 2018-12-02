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

    public bool Contains(string s)
    {
        foreach (StatusEffect se in effectList)
        {
            if (se.effect.IndexOf(s) != -1)
                return true;
        }
        return false;
    }

    public void Refresh(string s, int dur)
    {
        for(int i = 0; i < effectList.Count; i++)
        {
            if (effectList[i].effect.CompareTo(s) == 0)
                effectList[i].duration = dur;
        }
    }

    public void Add(string s, int dur = -1)
    {
        effectList.Add(new StatusEffect(s, dur));
    }

    public void Remove(string s)
    {
        for(int i = 0; i < effectList.Count; i++)
        {
            if (effectList[i].effect.IndexOf(s) != -1)
                effectList.RemoveAt(i);
        }
    }
}