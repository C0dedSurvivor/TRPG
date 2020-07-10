class DialogueCanPlayerMove : DialogueNode
{
    public bool canMove;

    public DialogueCanPlayerMove(bool canMove, DialogueNode nextNode = null) : base(nextNode)
    {
        this.canMove = canMove;
    }

    public override DialogueNode GetNext()
    {
        return nextNode;
    }
}
