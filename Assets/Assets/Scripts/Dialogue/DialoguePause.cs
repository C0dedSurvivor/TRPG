using System.Collections.Generic;

class DialoguePause : DialogueNode
{
    //How long to pause for
    public float seconds;

    public DialoguePause(float seconds, List<DialogueNode> path = null) : base(path)
    {
        this.seconds = seconds;
    }

    public override DialogueNode GetNext()
    {
        return dialoguePath?[0];
    }
}
