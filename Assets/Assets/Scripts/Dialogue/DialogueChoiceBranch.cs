using System;
using System.Collections.Generic;

class DialogueChoiceBranch : DialogueNode
{
    private List<DialogueBranchInfo> options = new List<DialogueBranchInfo>();

    public DialogueChoiceBranch(List<DialogueBranchInfo> options) : base(null)
    {
        this.options = options;
    }

    public List<string> GetOptions()
    {
        List<string> openOptions = new List<string>();
        foreach (DialogueBranchInfo branch in options)
        {
            if (branch.condition == null || branch.condition.Evaluate())
                openOptions.Add(branch.choiceText);
        }
        return openOptions;
    }

    public override DialogueNode GetNext()
    {
        throw new Exception("Tried to grab invalid next node from branching dialogue");
    }

    public DialogueNode GetNext(string option)
    {
        foreach (DialogueBranchInfo branch in options)
        {
            if (branch.choiceText == option)
                return branch.nextNode;
        }
        throw new Exception("Given branch not found.");
    }
}