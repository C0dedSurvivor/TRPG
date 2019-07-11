using UnityEngine;

public class Enemy : BattlePawnBase{
    //Will be used later for advanced AIs
    //See Battle's MoveEnemies() for more information
    int packVar;
    int aggro;
    
    public Enemy(string name, int x, int y, int mT, int aggresion, int pack) : base(x, y, mT, name)
    {
        aggro = aggresion;
        packVar = pack;
    }
}
