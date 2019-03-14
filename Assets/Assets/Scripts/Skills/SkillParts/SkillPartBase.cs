using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargettingType
{
    Self,
    Ally,
    Enemy,
    All
}

public class SkillPartBase {
    public TargettingType targetType;

    //1-100
    public int chanceToProc;

    public SkillPartBase() { } 
}
