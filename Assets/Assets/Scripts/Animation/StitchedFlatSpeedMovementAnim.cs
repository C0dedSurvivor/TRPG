using System.Collections.Generic;
using UnityEngine;

public class StitchedFlatSpeedMovementAnim : MovementAnimBase
{
    private List<Pair<Vector3, float>> positions;
    private float totalPercent = 0;

    public Vector3 difference => positions[0].First - positions[positions.Count - 1].First;
    public float totalDistance => positions[positions.Count - 1].Second;

    public StitchedFlatSpeedMovementAnim(GameObject target, float speed, List<Vector3> positions, bool concurrent = false) :
        base(target, speed, positions[positions.Count - 1], concurrent)
    {
        this.positions = new List<Pair<Vector3, float>>();

        for(int i = 0; i < positions.Count; i++)
        {
            this.positions.Add(new Pair<Vector3, float>(positions[i], i == 0 ? 0 : Vector3.Distance(positions[i], positions[i - 1]) + this.positions[i - 1].Second));
        }
    }

    public override void StepAnimation()
    {
        totalPercent += speed * Time.deltaTime;
        float newDistance = Mathf.Clamp(totalDistance * totalPercent, 0, totalDistance);

        int firstPosition = 0;
        int secondPosition = 1;
        for (int i = 0; i < positions.Count; i++)
        {
            if(positions[i].Second >= newDistance)
            {
                firstPosition = i - 1;
                secondPosition = i;
                break;
            }
        }

        float subPercent = (newDistance - positions[firstPosition].Second) / (positions[secondPosition].Second - positions[firstPosition].Second);
        mover.transform.position = Vector3.Lerp(
            positions[firstPosition].First, 
            positions[secondPosition].First, 
            subPercent
        );
    }

    public override bool IsDone()
    {
        return totalPercent >= 1.0;
    }
}
