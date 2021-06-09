using System.Collections.Generic;

public enum QuestState
{
    Inactive,
    Incomplete,
    ReadyForSubmission,
    Complete
}

/// <summary>
/// The stored current progress on a quest
/// </summary>
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

    /// <summary>
    /// Checks if a given integer matches this instance's referenced quest ID
    /// Used to see if a quest has a currently active instance
    /// </summary>
    /// <param name="quest">This</param>
    /// <param name="ID">The quest ID to check</param>
    /// <returns>Whether the given ID matches this instance's stored ID</returns>
    public static bool operator ==(QuestInstanceData quest, int ID)
    {
        return ID == quest.questID;
    }
    public static bool operator !=(QuestInstanceData quest, int ID)
    {
        return ID != quest.questID;
    }
}