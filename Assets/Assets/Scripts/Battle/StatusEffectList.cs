using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectList
{
    private List<Pair<Statuses, int>> effectList;

    public StatusEffectList()
    {
        effectList = new List<Pair<Statuses, int>>();
    }

    /// <summary>
    /// Returns if the list contains a certain status condition
    /// </summary>
    /// <param name="status">The status effect to check for</param>
    public bool Contains(Statuses status)
    {
        foreach (Pair<Statuses, int> se in effectList)
        {
            if (se.First == status)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a new status effect to the list if it isn't there already, or changes the limiter on the status if there is one
    /// </summary>
    /// <param name="status">The status to add</param>
    /// <param name="limit">The limit of the status, -1 if there is not a limit</param>
    public void Add(Statuses status, int limit)
    {
        Pair<Statuses, int> inList = null;
        foreach (Pair<Statuses, int> se in effectList)
        {
            if (se.First == status)
                inList = se;
        }

        if (inList == null)
        {
            effectList.Add(new Pair<Statuses, int>(status, limit));
        }
        else if (!(Registry.StatusEffectRegistry[status].limit != CountdownType.None))
        {
            inList.Second = Mathf.Max(inList.Second, limit);
        }
        
    }

    /// <summary>
    /// Removes a given status from the list
    /// </summary>
    /// <param name="status">Status effect to remove</param>
    public void Remove(Statuses status)
    {
        for(int i = 0; i < effectList.Count; i++)
        {
            if (effectList[i].First == status)
                effectList.RemoveAt(i);
        }
    }

    public void EndOfTurn()
    {
        for(int i = 0; i < effectList.Count; i++)
        {
            if (Registry.StatusEffectRegistry[effectList[i].First].limit == CountdownType.Turns)
            {
                effectList[i].Second--;
                if(effectList[i].Second == 0)
                {
                    effectList.RemoveAt(i);
                    i--;
                }
            }
            //If the effect has a chance to be removed at the end of the turn
            if(Random.value < Registry.StatusEffectRegistry[effectList[i].First].endOfTurnRemoveChance)
            {
                effectList.RemoveAt(i);
                i--;
            }
        }
    }

    public void EndOfMatch()
    {
        for (int i = 0; i < effectList.Count; i++)
        {
            if (!Registry.StatusEffectRegistry[effectList[i].First].persists)
            {
                effectList.RemoveAt(i);
                i--;
            }
        }
    }
}