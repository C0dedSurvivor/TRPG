using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TurnEvent : BattleEventBase
{
    public BattleParticipant turner;
    public FacingDirection direction;

    public TurnEvent(BattleParticipant turner, FacingDirection direction)
    {
        this.turner = turner;
        this.direction = direction;
    }
}
