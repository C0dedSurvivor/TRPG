using UnityEngine;

class FlatSpeedRotationAnim : RotationAnimBase
{
    private Quaternion initialRotation;
    private float percent = 0;

    public FlatSpeedRotationAnim(GameObject target, float speed, Quaternion initialRot, Quaternion finalRot, bool concurrent = false) : 
        base(target, speed, finalRot, concurrent)
    {
        initialRotation = initialRot;
    }

    public override void StepAnimation()
    {
        percent += speed * Time.deltaTime;
        mover.transform.rotation = Quaternion.Lerp(initialRotation, finalRotation, percent);
    }

    public override bool IsDone()
    {
        return percent >= 1.0;
    }
}