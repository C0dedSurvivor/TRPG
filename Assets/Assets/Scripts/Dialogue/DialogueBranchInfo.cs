public class DialogueBranchInfo
{
    public string choiceText;
    public ConditionalCheck condition;
    public bool showIfLocked;
    public DialogueNode nextNode;

    /// <summary>
    /// Constructor for a DialogueChoiceBranch branch
    /// </summary>
    public DialogueBranchInfo(string choiceText, DialogueNode nextNode)
    {
        this.choiceText = choiceText;
        this.nextNode = nextNode;
        condition = null;
        showIfLocked = false;
    }

    /// <summary>
    /// Constructor for a DialogueConditionalBranch branch
    /// </summary>
    public DialogueBranchInfo(ConditionalCheck condition, DialogueNode nextNode)
    {
        this.nextNode = nextNode;
        this.condition = condition;
        showIfLocked = false;
        choiceText = null;
    }

    /// <summary>
    /// Constructor for a DialogueLockedChoiceBranch branch
    /// </summary>
    public DialogueBranchInfo(string choiceText, ConditionalCheck condition, bool showIfLocked, DialogueNode nextNode)
    {
        this.choiceText = choiceText;
        this.condition = condition;
        this.showIfLocked = showIfLocked;
        this.nextNode = nextNode;
    }
}