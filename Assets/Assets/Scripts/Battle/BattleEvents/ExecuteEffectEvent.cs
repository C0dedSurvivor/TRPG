using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteEffectEvent : BattleEventBase
{
    public SkillPartBase effect;
    public BattleParticipant caster;
    public BattleParticipant target;
    public bool fromSpell;
    public int valueFromPrevious;

    public ExecuteEffectEvent(SkillPartBase effect, BattleParticipant caster, BattleParticipant target, bool fromSpell = false, int valueFromPrevious = -1)
    {
        this.effect = effect;
        this.caster = caster;
        this.target = target;
        this.fromSpell = fromSpell;
        this.valueFromPrevious = valueFromPrevious;
    }
}
