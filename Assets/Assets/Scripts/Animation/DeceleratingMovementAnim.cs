using UnityEngine;

public class DeceleratingMovementAnim : MovementAnimBase
{
    public DeceleratingMovementAnim(GameObject target, float speed, Vector3 finalPos, bool concurrent = false) : 
        base(target, speed, finalPos, concurrent) { }

    public override void StepAnimation()
    {
        mover.transform.position = Vector3.Lerp(mover.transform.position, finalPosition, speed * Time.deltaTime);
    }

    public override bool IsDone()
    {
        return GameStorage.Approximately(mover.transform.position, finalPosition);
    }

    public override void FinalizeAnim()
    {
        mover.transform.position = finalPosition;
    }
}