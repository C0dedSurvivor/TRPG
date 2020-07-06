using System.Collections.Generic;

class DialogueVisibleSelf : DialogueNode
{
    public bool newVisibility;

    public DialogueVisibleSelf(bool newVisibility, List<DialogueNode> path = null) : base(path)
    {
        this.newVisibility = newVisibility;
    }

    public override DialogueNode GetNext()
    {
        return dialoguePath?[0];
    }
}
