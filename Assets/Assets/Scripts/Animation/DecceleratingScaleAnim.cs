using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DecceleratingScaleAnim : AnimBase
{
    protected float speed;
    protected Vector3 finalScale;

    public DecceleratingScaleAnim(GameObject target, float speed, float uniformFinalScale, bool concurrent = false) : base(target, concurrent)
    {
        this.speed = speed;
        finalScale = new Vector3(uniformFinalScale, uniformFinalScale, uniformFinalScale);
    }

    public DecceleratingScaleAnim(GameObject target, float speed, Vector3 finalScale, bool concurrent = false) : base(target, concurrent)
    {
        this.speed = speed;
        this.finalScale = finalScale;
    }

    public override void StepAnimation()
    {
        mover.transform.localScale = Vector3.Lerp(mover.transform.localScale, finalScale, speed * Time.deltaTime);
    }

    public override bool IsDone()
    {
        return GameStorage.Approximately(mover.transform.localScale, finalScale);
    }

    public override void FinalizeAnim()
    {
        mover.transform.localScale = finalScale;
    }
}
