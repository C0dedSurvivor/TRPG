class QuestProgressConditional : ConditionalCheck
{
    private int questID;
    private QuestState state;

    public QuestProgressConditional(int questID, QuestState state)
    {
        this.questID = questID;
        this.state = state;
    }

    public bool Evaluate()
    {
        return QuestManager.Instance.GetQuestStatus(questID) == state;
    }
}