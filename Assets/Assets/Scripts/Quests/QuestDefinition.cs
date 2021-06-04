using System.Collections.Generic;

public class QuestDefinition
{
    public List<QuestObjectiveDef> objectives;
    public bool repeatable;

    public QuestDefinition(List<QuestObjectiveDef> objectives, bool repeatable)
    {
        this.objectives = objectives;
        this.repeatable = repeatable;
    }
}