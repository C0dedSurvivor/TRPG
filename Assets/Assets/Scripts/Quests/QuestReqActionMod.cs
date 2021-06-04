using System;

public enum QuestActionModType
{
    Who,
    Where,
    Target,
    Using
}

public class QuestReqActionMod : IEquatable<QuestReqActionMod>
{
    public QuestActionModType type;
    public string mod;

    public QuestReqActionMod(QuestActionModType type, string mod)
    {
        this.type = type;
        this.mod = mod;
    }

    public bool Equals(QuestReqActionMod other)
    {
        return other.type == type && other.mod == mod;
    }
}