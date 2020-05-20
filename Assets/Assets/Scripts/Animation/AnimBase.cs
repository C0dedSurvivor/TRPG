using UnityEngine;

public abstract class AnimBase
{
    //Whether ot not this animation is supposed to be run at the same time as other animations
    public bool concurrent;
    //What object is being animated
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

    /// <summary>
    /// Returns if the object being animated has reached its ending state
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDone() { return false; }

    /// <summary>
    /// Called when animation is finished, makes sure everything ends where it should
    /// </summary>
    public virtual void FinalizeAnim() { }
}