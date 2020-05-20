using System.Collections.Generic;
using UnityEngine;

public class BattleEventQueue
{
    List<BattleEventBase> events;
    //The head of the queue, aka where to insert at
    int insertionIndex;

    public int Count { get { return events.Count; } }

    public BattleEventQueue()
    {
        events = new List<BattleEventBase>();
        insertionIndex = 0;
    }

    /// <summary>
    /// Inserts an event at the head of the queue
    /// </summary>
    /// <param name="eventToInsert">Event to add to the queue</param>
    public void Insert(BattleEventBase eventToInsert)
    {
        events.Insert(insertionIndex, eventToInsert);
        insertionIndex++;
    }

    /// <summary>
    /// Returns the next event, dequeueing it and resetting the head
    /// </summary>
    /// <returns>The next event</returns>
    public BattleEventBase GetNext()
    {
        BattleEventBase eventToReturn = events[0];
        events.RemoveAt(0);
        insertionIndex = 0;
        return eventToReturn;
    }

    /// <summary>
    /// Checks to see if the next event is another movement event for a given object
    /// </summary>
    /// <param name="movedObject">What object are we checking for</param>
    /// <returns>True if the next event is a movement event moving the given object</returns>
    public bool StillMoving(GameObject movedObject)
    {
        return events.Count != 0 && events[0] is MovementEvent && (events[0] as MovementEvent).mover == movedObject;
    }

    /// <summary>
    /// Checks to see if the next event is an animation that should be playing alongside the current one
    /// </summary>
    /// <returns>True if the next event is a movement event and concurrent</returns>
    public bool NextIsConcurrent()
    {
        return events.Count != 0 && events[0] is MovementEvent && (events[0] as MovementEvent).animation.concurrent;
    }
}
