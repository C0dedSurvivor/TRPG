using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public int buttonID;

    public Battle battle;

    /// <summary>
    /// Lets the battle script know when the player starts mousing over the skill button
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battle.selectedSpell != buttonID && battle.hoveredSpell != buttonID)
            battle.HoveringSpell(buttonID);
    }

    /// <summary>
    /// Lets the battle script know when the player stops mousing over the skill button
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        battle.StopHoveringSpell();
    }
}
