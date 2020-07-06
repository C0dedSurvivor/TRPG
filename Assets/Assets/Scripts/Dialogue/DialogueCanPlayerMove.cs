using System.Collections.Generic;

class DialogueCanPlayerMove : DialogueNode
{
    public bool canMove;

    public DialogueCanPlayerMove(bool canMove, List<DialogueNode> path = null) : base(path)
    {
        this.canMove = canMove;
    }

    public override DialogueNode GetNext()
    {
        return dialoguePath?[0];
    }
}
