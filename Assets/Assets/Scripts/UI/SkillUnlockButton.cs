using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillUnlockButton : MonoBehaviour
{
    public int skillID;
    public SkillTreeGUI guiController;

    public int GetUnlockState()
    {
        if (GetComponent<Button>().image.color == Color.grey)
            return 1;
        if (GetComponent<Button>().image.color == Color.green)
            return 2;
        if (GetComponent<Button>().image.color == Color.red)
            return 3;
        return 0;
    }

    public void Clicked()
    {
        guiController.SkillInteraction(skillID);
    }
}
