public class DialogueBranchInfo
{
    public string choiceText;
    public ConditionalCheck condition;
    public bool showIfLocked;
    public DialogueNode nextNode;

    /// <summary>
    /// Constructor for a branch where the player can always choose it
    /// </summary>
    public DialogueBranchInfo(string choiceText, DialogueNode nextNode)
    {
        this.choiceText = choiceText;
        this.nextNode = nextNode;
        condition = null;
        showIfLocked = false;
    }

    /// <summary>
    /// Constructor for a branch for an automated check to see what path to go down
    /// </summary>
    public DialogueBranchInfo(ConditionalCheck condition, DialogueNode nextNode)
    {
        this.nextNode = nextNode;
        this.condition = condition;
        showIfLocked = false;
        choiceText = null;
    }

    /// <summary>
    /// Constructor for the default branch for an automated check for if no other paths are valid
    /// </summary>
    public DialogueBranchInfo(DialogueNode nextNode)
    {
        this.nextNode = nextNode;
        condition = null;
        showIfLocked = false;
        choiceText = null;
    }

    /// <summary>
    /// Constructor for a branch that can only be chosen if a certain condition is met
    /// </summary>
    public DialogueBranchInfo(string choiceText, ConditionalCheck condition, bool showIfLocked, DialogueNode nextNode)
    {
        this.choiceText = choiceText;
        this.condition = condition;
        this.showIfLocked = showIfLocked;
        this.nextNode = nextNode;
    }
}