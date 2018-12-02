using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//allows the player to select between two options
public class TwoButtonPopup : MonoBehaviour {
    public Text mainText;
    public Text leftText;
    public Text rightText;
    //1 = left button, 2 = right button
    public int selected = 0;

	public void ButtonClicked(int LoR)
    {
        selected = LoR;
    }
}