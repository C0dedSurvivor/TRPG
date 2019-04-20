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

    public bool StillMoving(GameObject movedObject)
    {
        return events.Count != 0 && events[0] is MovementEvent && (events[0] as MovementEvent).mover == movedObject;
    }

    public bool NextIsConcurrent()
    {
        return events.Count != 0 && events[0] is MovementEvent && (events[0] as MovementEvent).animation.concurrent;
    }
}
