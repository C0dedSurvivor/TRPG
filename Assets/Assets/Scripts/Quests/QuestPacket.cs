using System.Collections.Generic;

/// <summary>
/// A packet of action information used to check if a quest is progressed
/// </summary>
public class QuestPacket
{
    public LoggableAction action;
    public List<QuestReqActionMod> mods;
    public float amount;

    public QuestPacket(LoggableAction action, List<QuestReqActionMod> mods, float amount)
    {
        this.action = action;
        this.mods = mods;
        this.amount = amount;
    }
}