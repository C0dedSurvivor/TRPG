using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
class DecceleratingRotationAnim : AnimBase
{
    protected float speed;
    protected Quaternion finalRotation;

    public DecceleratingRotationAnim(GameObject target, float speed, Quaternion finalRot, bool concurrent = false) : base(target, concurrent)
    {
        this.speed = speed;
        finalRotation = finalRot;
    }

    public override void StepAnimation()
    {
        mover.transform.rotation = Quaternion.Lerp(mover.transform.rotation, finalRotation, speed * Time.deltaTime);
    }

    public override bool IsDone()
    {
        return GameStorage.Approximately(mover.transform.rotation, finalRotation);
    }

    public override void FinalizeAnim()
    {
        mover.transform.rotation = finalRotation;
    }
}
