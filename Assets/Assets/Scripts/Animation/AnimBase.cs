using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AnimBase
{
    public bool concurrent;
    public GameObject mover;

    protected AnimBase(GameObject target, bool concurrent)
    {
        mover = target;
        this.concurrent = concurrent;
    }

    public virtual void StepAnimation() { }
    public virtual bool IsDone() { return false; }
    public virtual void FinalizeAnim() { }
}