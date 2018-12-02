using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPartBase {

    public string skillPartType;

    //1 = self, 2 = enemy, 3 = ally, 4 = passive, 5 = all
    public int targetType;

    //1-100
    public int chanceToProc;

    public SkillPartBase() { } 
}
