using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The different unique effects that exist
/// </summary>
public enum UniqueEffects
{
    MoveAgain
}

/// <summary>
/// This effect part stores effects that are, for one reason or another, unable to be made modular in any way
/// </summary>
public class UniqueEffectPart : SkillPartBase
{
    public UniqueEffects effect;

    public UniqueEffectPart(TargettingType target, UniqueEffects effect, int chance = 100) : base(target, chance)
    {
        this.effect = effect;
    }
}
