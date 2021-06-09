using System.Collections.Generic;

/// <summary>
/// A list of actions that can be monitored by a quest
/// </summary>
public enum LoggableAction
{
    RiseAbove25Percent,
    RiseAbove50Percent,
    TakeDamage,
    DealDamage,
    TakePhysicalDamage,
    DealPhysicalDamage,
    TakeMagicDamage,
    DealMagicDamage,
    BasicAttack,
    HitWithBasicAttack,
    SpellCast,
    DealSpellDamage,
    HealWithSpell,
    KillAnEnemy,
    Die,
    GettingHealed,
    Healing,
    StartOfBattle,
    StartOfTurn,
    EndOfTurn,
    EndOfBattle,

    EnterArea,
    LeaveArea,
    GetCurrency,
    GetItem,
    TalkToNPC,
    LevelUp,
    Travel,
    Purchase,
    GetSpell,
    EquipItem
}

/// <summary>
/// Denotes the formatting to use when displaying the completion progress
/// </summary>
public enum QuestMeasures
{
    Currency,
    Distance,
    Repeats,
    Health,
    Mana,
    Tiles
}

public class QuestObjectiveDef
{
    public string description;
    public LoggableAction action;
    //All these modifiers are needed for action to count towards progression
    public List<QuestReqActionMod> requiredMods;
    //Having any of these modifiers mean quest is not progressed by the action
    public List<QuestReqActionMod> disqualifyingMods;
    public float completionReqAmt;
    //Determines what prefix and suffix to give the completion measure if any
    public QuestMeasures completionMeasure;

    public QuestObjectiveDef(string description, LoggableAction action, List<QuestReqActionMod> requiredMods, List<QuestReqActionMod> disqualifyingMods, float completionReqAmt, QuestMeasures completionMeasure)
    {
        this.description = description;
        this.action = action;
        this.requiredMods = requiredMods;
        this.disqualifyingMods = disqualifyingMods;
        this.completionReqAmt = completionReqAmt;
        this.completionMeasure = completionMeasure;
    }
}