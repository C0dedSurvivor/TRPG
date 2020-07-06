using System.Collections.Generic;
using UnityEngine;

public class DialogueConditionalBranch : DialogueNode
{
    List<ConditionalCheck> conditionals = new List<ConditionalCheck>();

    public DialogueConditionalBranch(List<ConditionalCheck> conditionals, List<DialogueNode> path = null) : base(path)
    {
        this.conditionals = conditionals;
    }

    public override DialogueNode GetNext()
    {
        for (int i = 0; i < conditionals.Count; i++)
        {
            if (conditionals[i].Evaluate())
                return dialoguePath[i];
        }
        Debug.Log("No path forward found, ending the dialogue");
        return null;
    }
}