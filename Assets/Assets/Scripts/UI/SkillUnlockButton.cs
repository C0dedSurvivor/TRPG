using UnityEngine;
using UnityEngine.UI;

public class SkillUnlockButton : MonoBehaviour
{
    /// <summary>
    /// What skill this button represents
    /// </summary>
    public int skillID;
    public SkillTreeGUI guiController;

    /// <summary>
    /// Checks whether this skill is unlocked, unlockable, or not unlockable
    /// </summary>
    /// <returns> 1 for unlocked, 2 for unlockable, 3 for not unlockable, 0 for error </returns>
    public int GetUnlockState()
    {
        //Unlocked
        if (GetComponent<Button>().image.color == Color.grey)
            return 1;
        //Unlockable
        if (GetComponent<Button>().image.color == Color.green)
            return 2;
        //Not unlockable
        if (GetComponent<Button>().image.color == Color.red)
            return 3;
        //Broken
        return 0;
    }

    /// <summary>
    /// When this button is clicked, tell the skill tree controller
    /// </summary>
    public void Clicked()
    {
        guiController.SkillInteraction(skillID);
    }
}
