using System.Collections.Generic;

public class MovementType
{
    public string name { get; set; }
    /// <summary>
    /// The IDs of tile types this movement type can go over, along with the restrictions of doing such
    /// </summary>
    public Dictionary<int, MovementTypeTileInfo> tileTypeInfo;
    public int moveSpeed;

    public MovementType(string name, Dictionary<int, MovementTypeTileInfo> tileTypeInfo, int moveSpeed)
    {
        this.name = name;
        this.tileTypeInfo = tileTypeInfo;
        this.moveSpeed = moveSpeed;
    }
}