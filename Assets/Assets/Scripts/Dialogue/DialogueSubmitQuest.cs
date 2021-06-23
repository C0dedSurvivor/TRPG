class DialogueSubmitQuest : DialogueNode
{
    //What quest to submit
    public int questID;

    public DialogueSubmitQuest(int questID, DialogueNode nextNode = null) : base(nextNode)
    {
        this.questID = questID;
    }

    public override DialogueNode GetNext()
    {
        return nextNode;
    }
}