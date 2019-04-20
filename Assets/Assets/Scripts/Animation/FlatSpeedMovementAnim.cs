﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class FlatSpeedMovementAnim : DecceleratingMovementAnim
{
    private Vector3 initialPosition;
    private float percent = 0;
    
    public Vector3 Difference { get { return initialPosition - finalPosition; } }

    public FlatSpeedMovementAnim(GameObject target, float speed, Vector3 initialPos, Vector3 finalPos, bool concurrent = false) : base(target, speed, finalPos, concurrent)
    {
        initialPosition = initialPos;
    }

    public override void StepAnimation()
    {
        percent += speed * Time.deltaTime;
        mover.transform.position = Vector3.Lerp(initialPosition, finalPosition, percent);
    }

    public override bool IsDone()
    {
        return percent >= 1.0;
    }
}
