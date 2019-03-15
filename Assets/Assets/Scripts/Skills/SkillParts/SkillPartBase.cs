using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargettingType
{
    Self,
    //Care about effect range
    Ally,
    Enemy,
    AllInRange,
    //Don't care about effect range
    AllAllies,
    AllAlliesNotSelf,
    AllEnemies
}

public class SkillPartBase {
    public TargettingType targetType;

    //1-100
    public int chanceToProc;

    public SkillPartBase() { } 
}
