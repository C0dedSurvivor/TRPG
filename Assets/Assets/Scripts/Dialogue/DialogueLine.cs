using System.Collections.Generic;

public class DialogueLine : DialogueNode
{
    public string speaker;
    public string line;

    public DialogueLine(string line, List<DialogueNode> path = null) : base(path)
    {
        speaker = "";
        this.line = line;
    }

    public DialogueLine(string speaker, string line, List<DialogueNode> path = null) : base(path)
    {
        this.speaker = speaker;
        this.line = line;
    }

    public override DialogueNode GetNext()
    {
        return dialoguePath?[0];
    }
}