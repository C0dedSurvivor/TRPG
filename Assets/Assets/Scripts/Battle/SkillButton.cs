using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public int buttonID;

    public Battle battle;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(battle.selectedSpell != buttonID && battle.hoveredSpell != buttonID)
            battle.HoveringSpell(buttonID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        battle.StopHoveringSpell();
    }
}
