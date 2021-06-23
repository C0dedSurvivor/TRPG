class DialogueAcceptQuest : DialogueNode
{
    //What quest to accept
    public int questID;

    public DialogueAcceptQuest(int questID, DialogueNode nextNode = null) : base(nextNode)
    {
        this.questID = questID;
    }

    public override DialogueNode GetNext()
    {
        return nextNode;
    }
}