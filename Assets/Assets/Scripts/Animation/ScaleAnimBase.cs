using UnityEngine;

public abstract class ScaleAnimBase : AnimBase
{
    protected float speed;
    protected Vector3 finalScale;

    public ScaleAnimBase(GameObject target, float speed, float uniformFinalScale, bool concurrent = false) :
        base(target, concurrent)
    {
        this.speed = speed;
        finalScale = new Vector3(uniformFinalScale, uniformFinalScale, uniformFinalScale);
    }

    public ScaleAnimBase(GameObject target, float speed, Vector3 finalScale, bool concurrent = false) :
        base(target, concurrent)
    {
        this.speed = speed;
        this.finalScale = finalScale;
    }
}