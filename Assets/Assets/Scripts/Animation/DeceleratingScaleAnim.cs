using UnityEngine;

public class DeceleratingScaleAnim : ScaleAnimBase
{
    public DeceleratingScaleAnim(GameObject target, float speed, float uniformFinalScale, bool concurrent = false) :
        base(target, speed, uniformFinalScale, concurrent)
    { }

    public DeceleratingScaleAnim(GameObject target, float speed, Vector3 finalScale, bool concurrent = false) :
        base(target, speed, finalScale, concurrent)
    { }

    public override void StepAnimation()
    {
        mover.transform.localScale = Vector3.Lerp(mover.transform.localScale, finalScale, speed * Time.deltaTime);
    }

    public override bool IsDone()
    {
        return GameStorage.Approximately(mover.transform.localScale, finalScale);
    }

    public override void FinalizeAnim()
    {
        mover.transform.localScale = finalScale;
    }
}
