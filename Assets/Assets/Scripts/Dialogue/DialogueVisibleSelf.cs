class DialogueVisibleSelf : DialogueNode
{
    public bool newVisibility;

    public DialogueVisibleSelf(bool newVisibility, DialogueNode nextNode = null) : base(nextNode)
    {
        this.newVisibility = newVisibility;
    }

    public override DialogueNode GetNext()
    {
        return nextNode;
    }
}
