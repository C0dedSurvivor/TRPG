using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FlatSpeedScaleAnim : DecceleratingScaleAnim
{
    public Vector3 initialScale;
    private float percent = 0;

    public FlatSpeedScaleAnim(GameObject target, float speed, Vector3 initialScale, float uniformFinalScale, bool concurrent = false) : base(target, speed, uniformFinalScale, concurrent)
    {
        this.initialScale = initialScale;
    }

    public FlatSpeedScaleAnim(GameObject target, float speed, Vector3 initialScale, Vector3 finalScale, bool concurrent = false) : base(target, speed, finalScale, concurrent)
    {
        this.initialScale = initialScale;
    }

    public override void StepAnimation()
    {
        percent += speed * Time.deltaTime;
        mover.transform.localScale = Vector3.Lerp(initialScale, finalScale, percent);
    }

    public override bool IsDone()
    {
        return percent >= 1.0;
    }
}
