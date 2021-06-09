using System;

public enum QuestActionModType
{
    Who,
    Where,
    Target,
    Using
}

/// <summary>
/// A modifier put on an action to allow a more granulated definition
/// </summary>
public class QuestReqActionMod : IEquatable<QuestReqActionMod>
{
    public QuestActionModType type;
    public string mod;

    public QuestReqActionMod(QuestActionModType type, string mod)
    {
        this.type = type;
        this.mod = mod;
    }

    /// <summary>
    /// Checks if this mod matches another
    /// </summary>
    /// <param name="other">The mod to compare against</param>
    public bool Equals(QuestReqActionMod other)
    {
        return other.type == type && other.mod == mod;
    }
}