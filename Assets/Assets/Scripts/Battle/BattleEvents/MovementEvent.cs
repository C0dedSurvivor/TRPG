using System.Collections.Generic;
using UnityEngine;

public class MovementEvent : BattleEventBase
{
    public AnimBase animation;
    public bool forced;

    public GameObject mover { get { return animation.mover; } }

    public MovementEvent(GameObject target, float speed, Vector3 finalPos, bool concurrent = false, bool forced = false)
    {
        animation = new DeceleratingMovementAnim(target, speed, finalPos, concurrent);
        this.forced = forced;
    }

    public MovementEvent(GameObject target, float speed, Quaternion finalRot, bool concurrent = false, bool forced = false)
    {
        animation = new DeceleratingRotationAnim(target, speed, finalRot, concurrent);
        this.forced = forced;
    }

    public MovementEvent(GameObject target, float speed, Quaternion initialRot, Quaternion finalRot, bool concurrent = false, bool forced = false)
    {
        animation = new FlatSpeedRotationAnim(target, speed, initialRot, finalRot, concurrent);
        this.forced = forced;
    }

    public MovementEvent(GameObject target, float speed, List<Vector3> positions, bool concurrent = false, bool forced = false)
    {
        animation = new StitchedFlatSpeedMovementAnim(target, speed, positions, concurrent);
        this.forced = forced;
    }
}
