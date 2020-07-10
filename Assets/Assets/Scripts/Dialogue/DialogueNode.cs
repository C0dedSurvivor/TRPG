public abstract class DialogueNode
{
    public DialogueNode nextNode;

    protected DialogueNode(DialogueNode nextNode = null)
    {
        this.nextNode = nextNode;
    }

    public abstract DialogueNode GetNext();
}