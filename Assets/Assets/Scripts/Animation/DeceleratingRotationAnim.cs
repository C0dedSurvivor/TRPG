using UnityEngine;

class DeceleratingRotationAnim : RotationAnimBase
{
    public DeceleratingRotationAnim(GameObject target, float speed, Quaternion finalRot, bool concurrent = false) :
        base(target, speed, finalRot, concurrent)
    { }

    public override void StepAnimation()
    {
        mover.transform.rotation = Quaternion.Lerp(mover.transform.rotation, finalRotation, speed * Time.deltaTime);
    }

    public override bool IsDone()
    {
        return GameStorage.Approximately(mover.transform.rotation, finalRotation);
    }

    public override void FinalizeAnim()
    {
        mover.transform.rotation = finalRotation;
    }
}
