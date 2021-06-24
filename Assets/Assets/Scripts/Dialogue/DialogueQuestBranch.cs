using System.Collections.Generic;

class DialogueQuestBranch : DialogueConditionalBranch
{
    private int questID;

    public DialogueQuestBranch(
        int questID,
        DialogueNode notUnlocked, 
        DialogueNode inProgress, 
        DialogueNode submitting, 
        DialogueNode complete, 
        DialogueLine givable, 
        DialogueNode givableAccept, 
        DialogueNode givableDeny)
        : base(null)
    {
        this.questID = questID;

        //Sets up the choice for accepting or denying the quest
        givable.nextNode = new DialogueChoiceBranch
        (
            new List<DialogueBranchInfo>()
            {
                new DialogueBranchInfo
                (
                    "Accept",
                    new DialogueAcceptQuest(
                        questID,
                        givableAccept
                    )
                ),
                new DialogueBranchInfo
                (
                    "Deny",
                    givableDeny
                )
            }
        );

        //Sets up the automatic dialogue branching based on quest progression 
        conditionals = new List<DialogueBranchInfo>()
        {
            new DialogueBranchInfo(
                new QuestProgressConditional(questID, QuestState.Incomplete),
                inProgress
                ),
            new DialogueBranchInfo(
                new QuestProgressConditional(questID, QuestState.ReadyForSubmission),
                new DialogueSubmitQuest(questID, submitting)
                ),
            new DialogueBranchInfo(
                new QuestProgressConditional(questID, QuestState.Complete),
                complete
                ),
            new DialogueBranchInfo(
                    Registry.QuestRegistry[questID].assignmentCriteria,
                    givable
                ),
            //If it is not in progress and can't be started
            new DialogueBranchInfo(
                notUnlocked
                ),
        };
    }
}
