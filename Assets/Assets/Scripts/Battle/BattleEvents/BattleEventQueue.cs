using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEventQueue
{
    List<BattleEventBase> events;
    int insertionIndex;

    public int Count{ get { return events.Count; } }

    public BattleEventQueue()
    {
        events = new List<BattleEventBase>();
        insertionIndex = 0;
    }

    public void Insert(BattleEventBase eventToInsert)
    {
        events.Insert(insertionIndex, eventToInsert);
        insertionIndex++;
    }

    public BattleEventBase GetNext()
    {
        BattleEventBase eventToReturn = events[0];
        events.RemoveAt(0);
        insertionIndex = 0;
        return eventToReturn;
    }
}
