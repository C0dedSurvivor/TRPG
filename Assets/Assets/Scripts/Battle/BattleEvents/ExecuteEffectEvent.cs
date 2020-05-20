public class ExecuteEffectEvent : BattleEventBase
{
    public SkillPartBase effect;
    public BattlePawnBase caster;
    public BattlePawnBase target;
    public bool fromSpell;
    public int valueFromPrevious;

    public ExecuteEffectEvent(SkillPartBase effect, BattlePawnBase caster, BattlePawnBase target, bool fromSpell = false, int valueFromPrevious = -1)
    {
        this.effect = effect;
        this.caster = caster;
        this.target = target;
        this.fromSpell = fromSpell;
        this.valueFromPrevious = valueFromPrevious;
    }
}
