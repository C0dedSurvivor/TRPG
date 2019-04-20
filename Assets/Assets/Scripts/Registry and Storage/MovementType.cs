using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MovementType
{
    public string name;
    public List<BattleTiles> passableTiles;
    public bool hinderedByForest;
    public int moveSpeed;

    public MovementType(string name, List<BattleTiles> passableTiles, bool stoppedByForest, int moveSpeed)
    {
        this.name = name;
        this.passableTiles = passableTiles;
        hinderedByForest = stoppedByForest;
        this.moveSpeed = moveSpeed;
    }
}