using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour
{
    //The index of the item this button represents
    public int item;

    /// <summary>
    /// Updates what the button looks like if the item it represents is changed
    /// </summary>
    /// <param name="item">The new item to grab the info from</param>
	public void UpdateItem(StoredItem item)
    {
        //If this button has text as well as the image
        if (GetComponentInChildren<Text>() != null)
        {
            GetComponentsInChildren<Image>()[1].overrideSprite = Resources.Load<Sprite>("Images/ItemIcons/" + item.Name);
            GetComponentInChildren<Text>().text = item.Name + "    " + item.amount;
        }
        else
        {
            GetComponentInChildren<Image>().overrideSprite = Resources.Load<Sprite>("Images/ItemIcons/" + item.Name);
        }
    }
}
