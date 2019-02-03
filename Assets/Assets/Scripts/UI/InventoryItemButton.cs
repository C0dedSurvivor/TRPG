using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour {
    public int item;

	public void UpdateItem(StoredItem i)
    {
        if (GetComponentInChildren<Text>() != null)
        {
            GetComponentsInChildren<Image>()[1].overrideSprite = Resources.Load<Sprite>("Images/ItemIcons/" + i.Name);
            GetComponentInChildren<Text>().text = i.Name + "    " + i.amount;
        }
        else
        {
            GetComponentInChildren<Image>().overrideSprite = Resources.Load<Sprite>("Images/ItemIcons/" + i.Name);
        }
    }
}
