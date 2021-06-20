using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    private List<QuestInstanceData> currentQuests = new List<QuestInstanceData>();

    public Text questSidebarText;

    public static QuestManager qmInstance;

    /// <summary>
    /// Fun with singletons
    /// </summary>
    public static QuestManager Instance
    {
        get
        {
            if (qmInstance == null)
            {
                throw new Exception("Quest Manager not initialized in time.");
            }
            return qmInstance;
        }
    }

    /// <summary>
    /// Sets the instance in the scene as the singleton instance
    /// </summary>
    public void Awake()
    {
        if (qmInstance == null)
        {
            qmInstance = this;
        }
    }

    /// <summary>
    /// Adds a quest to the player's current quests if a quest with the same ID isn't already active
    /// </summary>
    /// <param name="questID">The ID of the quest to add</param>
    /// <returns>Returns whether the quest was successfully added</returns>
    public bool AcceptQuest(int questID)
    {
        foreach(QuestInstanceData quest in currentQuests)
        {
            if (quest == questID)
                return false;
        }
        currentQuests.Add(new QuestInstanceData(questID));
        return true;
    }

    /// <summary>
    /// Checks whether a given event progresses any active quests
    /// </summary>
    /// <param name="packet">Event data</param>
    public void CheckProgression(QuestPacket packet)
    {
        foreach(QuestInstanceData quest in currentQuests)
        {
            List<QuestObjectiveDef> objectives = Registry.QuestRegistry[quest.questID].objectives;
            for (int i = 0; i < objectives.Count; i++)
            {
                bool invalid = false;
                //If it has a mod that disqualifies it from progressing this objective, go to next objective
                foreach (QuestReqActionMod disqualifyingMod in objectives[i].disqualifyingMods)
                {
                    if (packet.mods.Contains(disqualifyingMod))
                    {
                        invalid = true;
                        break;
                    }
                }
                if (invalid)
                    continue;
                //If it is missing a mod required to progress this objective, go to next objective
                foreach (QuestReqActionMod requiredMod in objectives[i].requiredMods)
                {
                    if (!packet.mods.Contains(requiredMod))
                    {
                        invalid = true;
                        break;
                    }
                }
                if (invalid)
                    continue;
                quest.completionProgress[i] += packet.amount;
            }
        }
        questSidebarText.text = "";
        foreach (string s in GetCurrentQuestStrings()) { questSidebarText.text += s; }
    }

    /// <summary>
    /// Returns the string descriptions of all active quests for display
    /// </summary>
    /// <returns>List of quest descriptions</returns>
    public List<string> GetCurrentQuestStrings()
    {
        List<string> questStrings = new List<string>();
        foreach(QuestInstanceData quest in currentQuests)
        {
            string questString = "";
            QuestDefinition qDef = Registry.QuestRegistry[quest.questID];
            for(int i = 0; i < qDef.objectives.Count; i++)
            {
                questString += qDef.objectives[i].description + "\n" + quest.completionProgress[i] + "/" + qDef.objectives[i].completionReqAmt + "\n";
            }
            if (qDef.repeatable)
                questString += "Repeatable\n";
            questStrings.Add(questString);
        }
        return questStrings;
    }
}