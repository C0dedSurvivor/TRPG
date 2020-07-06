using System;
using System.Collections.Generic;

class DialogueChoiceBranch : DialogueNode
{
    List<string> choices = new List<string>();
    private int selected = -1;

    public DialogueChoiceBranch(List<string> choices, List<DialogueNode> path = null) : base(path)
    {
        this.choices = choices;
    }

    public void SetSelected(int choice)
    {
        selected = choice;
    }

    public override DialogueNode GetNext()
    {
        if(selected == -1)
            throw new Exception("Tried to grab dialogue choice before an option was selected");
        return dialoguePath[selected];
    }
}