using System.Collections.Generic;

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