using UnityEngine;

public class Enemy : BattleParticipant{
    //will be used for advanced AIs
    int packVar;
    int aggro;
    
    public Enemy(int x, int y, int mT, int aggresion, int pack) : base(x, y, mT)
    {
        aggro = aggresion;
        packVar = pack;
    }
}
