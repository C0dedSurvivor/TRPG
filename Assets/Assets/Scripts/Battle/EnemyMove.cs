using UnityEngine;

/// <summary>
/// Stores a possible enemy move
/// </summary>
public class EnemyMove
{
    //Where the pawn would move to
    public Vector2Int movePosition;
    //Where the pawn's attack target is
    public Vector2Int attackPosition;
    //How valuable this movement would be to make
    public float priority;
    //How valuable the reason this movement would be made is (attack someone, reposition, protect someone, etc, all have different values for this)
    public int reasonPriority;

    public EnemyMove(int x, int y, float priority, int reasonPriority)
    {
        movePosition = new Vector2Int(x, y);
        attackPosition = new Vector2Int(-1, -1);
        this.priority = priority;
        this.reasonPriority = reasonPriority;
    }

    public EnemyMove(int x, int y, int attackX, int attackY, float priority, int reasonPriority)
    {
        movePosition = new Vector2Int(x, y);
        attackPosition = new Vector2Int(attackX, attackY);
        this.priority = priority;
        this.reasonPriority = reasonPriority;
    }

    /// <summary>
    /// Determines which move has a higher priority
    /// </summary>
    /// <param name="m">The move to check this one against</param>
    public int CompareTo(EnemyMove m)
    {
        if (priority > m.priority)
        {
            return -1;
        }
        else if (priority < m.priority)
        {
            return 1;
        }
        else
        {
            if (reasonPriority > m.reasonPriority)
            {
                return -1;
            }
            else if (reasonPriority < m.reasonPriority)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}