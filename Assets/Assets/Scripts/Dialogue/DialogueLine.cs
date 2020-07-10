public class DialogueLine : DialogueNode
{
    public string speaker;
    public string line;

    public DialogueLine(string line, DialogueNode nextNode = null) : base(nextNode)
    {
        speaker = "";
        this.line = line;
    }

    public DialogueLine(string speaker, string line, DialogueNode nextNode = null) : base(nextNode)
    {
        this.speaker = speaker;
        this.line = line;
    }

    public override DialogueNode GetNext()
    {
        return nextNode;
    }
}