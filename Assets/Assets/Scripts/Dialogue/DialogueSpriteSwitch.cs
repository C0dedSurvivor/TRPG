using System.Collections.Generic;
using UnityEngine.UI;

public class DialogueSpriteSwitch : DialogueNode
{
    //What sprite you want to change
    public Image target;
    //The name of the new image to give it
    public string newTexture;

    public DialogueSpriteSwitch(Image target, string newTextureName, List<DialogueNode> path = null) : base(path)
    {
        this.target = target;
        newTexture = newTextureName;
    }

    public override DialogueNode GetNext()
    {
        return dialoguePath?[0];
    }
}
