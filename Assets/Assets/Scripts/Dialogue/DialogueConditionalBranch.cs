using System.Collections.Generic;
using UnityEngine;

public class DialogueConditionalBranch : DialogueNode
{
    private List<DialogueBranchInfo> conditionals = new List<DialogueBranchInfo>();

    public DialogueConditionalBranch(List<DialogueBranchInfo> conditionals) : base(null)
    {
        this.conditionals = conditionals;
    }

    public override DialogueNode GetNext()
    {
        foreach (DialogueBranchInfo branch in conditionals)
        {
            if (branch.condition == null || branch.condition.Evaluate())
                return branch.nextNode;
        }
        Debug.Log("No path forward found, ending the dialogue");
        return null;
    }
}