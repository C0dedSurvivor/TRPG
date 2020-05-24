using System.Collections.Generic;

public class MovementTypeTileInfo
{
    /// <summary>
    /// What direction this tile can be moved onto from.
    /// Only for Up, Down, Left, Right
    /// </summary>
    public Dictionary<MoveDirection, bool> canMoveInFrom = new Dictionary<MoveDirection, bool>();
    /// <summary>
    /// What direction this tile can be left in.
    /// Only for Up, Down, Left, Right
    /// </summary>
    public Dictionary<MoveDirection, bool> canMoveOutThrough = new Dictionary<MoveDirection, bool>();
    //Whether moving onto this tile will end the movement no matter the previous distance traveled
    public bool limitedMovement;
    //How much movement moving onto or through this tile takes
    public int moveDifficulty;

    public MovementTypeTileInfo() { }

    public MovementTypeTileInfo(Dictionary<MoveDirection, bool> canMoveInFrom, Dictionary<MoveDirection, bool> canMoveOutThrough, bool limitedMovement, int moveDifficulty)
    {
        this.canMoveInFrom = canMoveInFrom;
        this.canMoveOutThrough = canMoveOutThrough;
        this.limitedMovement = limitedMovement;
        this.moveDifficulty = moveDifficulty;
    }
}