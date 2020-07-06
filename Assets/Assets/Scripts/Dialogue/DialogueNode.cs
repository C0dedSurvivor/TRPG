using System.Collections.Generic;

public abstract class DialogueNode
{
    public List<DialogueNode> dialoguePath;

    protected DialogueNode(List<DialogueNode> path = null)
    {
        dialoguePath = path;
    }

    public abstract DialogueNode GetNext();
}