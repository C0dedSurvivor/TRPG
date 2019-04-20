using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class DecceleratingMovementAnim : AnimBase
{
    protected float speed;
    protected Vector3 finalPosition;

    public DecceleratingMovementAnim(GameObject target, float speed, Vector3 finalPos, bool concurrent = false) : base(target, concurrent)
    {
        this.speed = speed;
        finalPosition = finalPos;
    }

    public override void StepAnimation()
    {
        mover.transform.position = Vector3.Lerp(mover.transform.position, finalPosition, speed * Time.deltaTime);
    }

    public override bool IsDone()
    {
        return GameStorage.Approximately(mover.transform.position, finalPosition);
    }

    public override void FinalizeAnim()
    {
        mover.transform.position = finalPosition;
    }
}