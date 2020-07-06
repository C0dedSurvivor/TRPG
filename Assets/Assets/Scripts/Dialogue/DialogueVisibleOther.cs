using System.Collections.Generic;
using UnityEngine;

class DialogueVisibleOther : DialogueNode
{
    public GameObject target;
    public bool newVisibility;

    public DialogueVisibleOther(GameObject target, bool newVisibility, List<DialogueNode> path = null) : base(path)
    {
        this.target = target;
        this.newVisibility = newVisibility;
    }

    public override DialogueNode GetNext()
    {
        return dialoguePath?[0];
    }
}
