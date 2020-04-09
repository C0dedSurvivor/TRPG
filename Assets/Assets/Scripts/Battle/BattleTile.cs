using UnityEngine;

public class BattleTile
{
    public int tileType;

    public bool playerMoveRange = false;
    public bool enemyDanger = false;
    public bool playerAttackRange = false;
    public bool skillRange = false;
    public bool skillTargettable = false;
    public bool skillTargetting = false;

    public BattleTile(int tileType)
    {
        this.tileType = tileType;
    }

    /// <summary>
    /// Resets all the data in this tile
    /// </summary>
    public void Reset()
    {
        playerMoveRange = false;
        enemyDanger = false;
        playerAttackRange = false;
        skillRange = false;
        skillTargettable = false;
        skillTargetting = false;
    }
}
