/// <summary>
/// Template for an enemy
/// </summary>
public class EnemyType
{
    public string name;

    int level1Atk;
    int level1MAtk;
    int level1Def;
    int level1MDef;
    int level1Health;

    //Average stat growth per level, slightly randomized per level for variation
    int atkGrowth;
    int mAtkGrowth;
    int defGrowth;
    int mDefGrowth;
    int healthGrowth;
}