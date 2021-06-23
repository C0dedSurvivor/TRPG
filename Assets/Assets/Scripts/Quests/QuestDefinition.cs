using System.Collections.Generic;

/// <summary>
/// The immutable information connected to a quest ID
/// </summary>
public class QuestDefinition
{
    public List<QuestObjectiveDef> objectives;
    public bool repeatable;
    //The conditions necessary for the player to be able to get this quest
    public ConditionalCheck assignmentCriteria;

    public QuestDefinition(List<QuestObjectiveDef> objectives, bool repeatable, ConditionalCheck assignmentCriteria = null)
    {
        this.objectives = objectives;
        this.repeatable = repeatable;
        this.assignmentCriteria = assignmentCriteria ?? new TrueConditional();
    }
}