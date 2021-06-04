using System.Collections.Generic;

public enum QuestState
{
    Inactive,
    Incomplete,
    ReadyForSubmission,
    Complete
}

public class QuestInstanceData
{
    public int questID;
    public QuestState state;
    public List<float> completionProgress;

    public QuestInstanceData(int questID)
    {
        this.questID = questID;
        state = QuestState.Incomplete;
        completionProgress = new List<float>();
        for(int i = 0; i < Registry.QuestRegistry[questID].objectives.Count; i++) { completionProgress.Add(0); }
    }

    public static bool operator ==(QuestInstanceData quest, int ID)
    {
        return ID == quest.questID;
    }
    public static bool operator !=(QuestInstanceData quest, int ID)
    {
        return ID != quest.questID;
    }
}