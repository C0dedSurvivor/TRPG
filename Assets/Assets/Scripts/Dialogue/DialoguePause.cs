class DialoguePause : DialogueNode
{
    //How long to pause for
    public float seconds;

    public DialoguePause(float seconds, DialogueNode nextNode = null) : base(nextNode)
    {
        this.seconds = seconds;
    }

    public override DialogueNode GetNext()
    {
        return nextNode;
    }
}
