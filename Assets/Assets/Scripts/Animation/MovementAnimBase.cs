using UnityEngine;

public abstract class MovementAnimBase : AnimBase
{
    protected float speed;
    protected Vector3 finalPosition;

    public MovementAnimBase(GameObject target, float speed, Vector3 finalPos, bool concurrent = false) :
        base(target, concurrent)
    {
        this.speed = speed;
        finalPosition = finalPos;
    }
}