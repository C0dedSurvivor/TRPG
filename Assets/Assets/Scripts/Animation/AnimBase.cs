using UnityEngine;

public abstract class AnimBase
{
    public bool concurrent;
    public GameObject mover;

    protected AnimBase(GameObject target, bool concurrent)
    {
        mover = target;
        this.concurrent = concurrent;
    }

    /// <summary>
    /// Called in update, moves the object one frame in the animation
    /// </summary>
    public virtual void StepAnimation() { }

    public virtual bool IsDone() { return false; }

    /// <summary>
    /// Called when animation is finished, makes sure everything ends where it should
    /// </summary>
    public virtual void FinalizeAnim() { }
}