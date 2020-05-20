using UnityEngine;

public abstract class RotationAnimBase : AnimBase
{
    protected float speed;
    protected Quaternion finalRotation;

    public RotationAnimBase(GameObject target, float speed, Quaternion finalRot, bool concurrent = false) :
        base(target, concurrent)
    {
        this.speed = speed;
        finalRotation = finalRot;
    }
}