﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    private List<QuestInstanceData> currentQuests = new List<QuestInstanceData>();

    public Text questSidebarText;

    private static QuestManager qmInstance;

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
            {
                Debug.LogError("Tried to assign an already assigned quest ID.");
                return false;
            }
        }
        currentQuests.Add(new QuestInstanceData(questID));
        UpdateSidebar();
        return true;
    }

    /// <summary>
    /// Submits a quest if all relevant objectives are completed
    /// </summary>
    /// <param name="questID">The ID of the quest to submit</param>
    /// <returns>Returns whether the quest was successfully submitted</returns>
    public bool SubmitQuest(int questID)
    {
        foreach (QuestInstanceData quest in currentQuests)
        {
            if (quest == questID)
            {
                if (quest.state == QuestState.ReadyForSubmission)
                {
                    quest.state = QuestState.Complete;
                    UpdateSidebar();
                    return true;
                }
                else
                {
                    Debug.LogError("Tried to submit an unfinished quest.");
                    return false;
                }
            }
        }
        Debug.LogError("Tried to submit an unassigned quest.");
        return false;
    }

    /// <summary>
    /// Checks whether a given event progresses any active quests
    /// </summary>
    /// <param name="packet">Event data</param>
    public void CheckProgression(QuestPacket packet)
    {
        foreach(QuestInstanceData quest in currentQuests)
        {
            if (quest.state != QuestState.Complete && quest.state != QuestState.ReadyForSubmission)
            {
                bool questComplete = true;
                List<QuestObjectiveDef> objectives = Registry.QuestRegistry[quest.questID].objectives;
                for (int i = 0; i < objectives.Count; i++)
                {
                    if (objectives[i].action == packet.action) {
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
                        //Progresses the quest if it should be progressed
                        if (!invalid)
                            quest.completionProgress[i] = Mathf.Min(quest.completionProgress[i] + packet.amount, objectives[i].completionReqAmt);
                    }
                    //Checks if this objective is complete
                    if (questComplete && !GameStorage.Approximately(quest.completionProgress[i], objectives[i].completionReqAmt))
                        questComplete = false;
                }
                //If all objectives are complete, mark this as ready for turn in
                if (questComplete)
                    quest.state = QuestState.ReadyForSubmission;
            }
        }
        UpdateSidebar();
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
            if (quest.state != QuestState.Complete)
            {
                string questString = "";
                QuestDefinition qDef = Registry.QuestRegistry[quest.questID];
                for (int i = 0; i < qDef.objectives.Count; i++)
                {
                    questString += qDef.objectives[i].description + "\n" + quest.completionProgress[i] + "/" + qDef.objectives[i].completionReqAmt + "\n";
                }
                if (qDef.repeatable)
                    questString += "Repeatable\n";
                questStrings.Add(questString);
            }
        }
        return questStrings;
    }

    /// <summary>
    /// Updates the quest progress sidebar
    /// </summary>
    public void UpdateSidebar()
    {
        questSidebarText.text = "";
        foreach (string s in GetCurrentQuestStrings()) { questSidebarText.text += s; }
    }

    /// <summary>
    /// Checks the status of a given quest ID
    /// </summary>
    /// <param name="questID">The ID of the quest to check</param>
    /// <returns>Returns whether the quest was successfully added</returns>
    public QuestState GetQuestStatus(int questID)
    {
        foreach (QuestInstanceData quest in currentQuests)
        {
            if (quest == questID)
                return quest.state;
        }
        return QuestState.Inactive;
    }
}