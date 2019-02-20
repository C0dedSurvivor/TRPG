using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseInventory : GearInventoryGUI {
    //How this inventory should be sorted
    public Dropdown sorting;
    //How this inventory is filtered
    public Dropdown filter;
    //On click, tells this to update the visuals with a new filter and/or order of sorting
    public Button sortAndFilter;
    public Dropdown amtToDiscard;
    public Button discard;

    /// <summary>
    /// Gets the list of items and generates the visuals
    /// </summary>
    public override void GenerateInventory()
    {
        itemList = Inventory.GetItemList(filter.value - 1);
        base.GenerateInventory();
        sorting.gameObject.SetActive(true);
        filter.gameObject.SetActive(true);
        sortAndFilter.gameObject.SetActive(true);
    }

    /// <summary>
    /// Clears all of the visibles and data
    /// </summary>
    public override void Destroy()
    {
        base.Destroy();
        sorting.gameObject.SetActive(false);
        filter.gameObject.SetActive(false);
        sortAndFilter.gameObject.SetActive(false);
    }

    /// <summary>
    /// Rotates so the selected button is at the center of the wheel
    /// </summary>
    /// <param name="button">The ID of the button to center</param>
    public override void MoveToButton(int button)
    {
        base.MoveToButton(button);
        amtToDiscard.ClearOptions();
        if (Registry.ItemRegistry[itemList[selectedItem].Name] is EquippableBase)
        {
            //Doesn't matter what's in there when there's guarenteed only one item in the stack, just that there is something there
            amtToDiscard.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("Yes") });
            amtToDiscard.gameObject.SetActive(false);
        }
        else
        {
            //Adds in all of the options for the amount to discard (1 - amount owned)
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for (int i = 1; i <= itemList[selectedItem].amount; i++)
            {
                options.Add(new Dropdown.OptionData("" + i));
            }
            amtToDiscard.AddOptions(options);
            amtToDiscard.gameObject.SetActive(true);
        }
        discard.gameObject.SetActive(true);
    }

    /// <summary>
    /// Discards x amount of the selected item, where x is the value on the discard dropdown
    /// </summary>
    public void Discard()
    {
        itemList[selectedItem].amount -= amtToDiscard.value + 1;
        Inventory.RemoveItem(itemList[selectedItem].Name, amtToDiscard.value + 1);
        //If this removed all of that item from the inventory
        if (itemList[selectedItem].amount == 0)
        {
            SortAndChangeFilter();
            if (itemList.Count >= selectedItem + 1)
                MoveToButton(selectedItem + 1);
            else if (itemList.Count > 0)
                MoveToButton(itemList.Count);
            selectedItem = -1;
            amtToDiscard.gameObject.SetActive(false);
            amtToDiscard.value = 0;
            discard.gameObject.SetActive(false);
        }
        else
        {
            //Updates the item box visuals
            itemBoxList[selectedItem].GetComponent<InventoryItemButton>().UpdateItem(itemList[selectedItem]);
            //Update the discard amount dropdown for new values
            amtToDiscard.ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for (int i = 1; i <= itemList[selectedItem].amount; i++)
            {
                options.Add(new Dropdown.OptionData("" + i));
            }
            amtToDiscard.AddOptions(options);
            //Sets the currently selected amount to discard to be the previous amount or the maximum possible, whichever is lowest
            amtToDiscard.value = Mathf.Min(itemList[selectedItem].amount - 1, amtToDiscard.value);
        }
    }

    /// <summary>
    /// Sorts the inventory, grabs a list of items with the updated filter, and updates the visuals accordingly
    /// </summary>
    public void SortAndChangeFilter()
    {
        Inventory.SortInventory(sorting.value);
        ChangeInventory(filter.value - 1);
    }

    /// <summary>
    /// Displays and updates the item info card if the player is mousing over an item
    /// </summary>
    /// <param name="item">The index of the item you are mousing over</param>
    public override void MouseOverItem(int item)
    {
        itemInfo.SetActive(true);
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        //if (itemInfo.transform.localPosition.y < 0 && Mathf.Abs(itemInfo.transform.localPosition.y) + itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight > Screen.height / 2)
        //    itemInfo.transform.position = new Vector3(itemInfo.transform.position.x, Screen.height / 2 - itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight / 2, itemInfo.transform.position.z);
        itemInfo.transform.GetChild(0).GetComponent<Text>().text = itemList[item].Name;
        //Has to display extra stat information if the item is an equippable
        if (Registry.ItemRegistry[itemList[item].Name] is EquippableBase)
        {
            itemInfo.transform.GetChild(1).GetComponent<Text>().text = "Equipment type: ";
            switch (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).equipSlot)
            {
                case 0:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Weapon";
                    break;
                case 1:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Helmet";
                    break;
                case 2:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Chestplate";
                    break;
                case 3:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Legs";
                    break;
                case 4:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Boots";
                    break;
                case 5:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Hands";
                    break;
                case 6:
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "Accessory";
                    break;
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health != 0)
            {
                itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nHealth: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health;
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength != 0)
            {
                if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nPhysical";
                else
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nAEtheric";
                itemInfo.transform.GetChild(1).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength;
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense != 0)
            {
                if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nPhysical";
                else
                    itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nAEtheric";
                itemInfo.transform.GetChild(1).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense;
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod != 0)
            {
                itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nCrit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod + "%";
            }
            itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\n" + Registry.ItemRegistry[itemList[item].Name].FlavorText;
            itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nSells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;
        }
        else
        {
            itemInfo.transform.GetChild(1).GetComponent<Text>().text = itemList[item].amount + "/" + Registry.ItemRegistry[itemList[item].Name].MaxStack;
            itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\n" + Registry.ItemRegistry[itemList[item].Name].FlavorText;
            itemInfo.transform.GetChild(1).GetComponent<Text>().text += "\nSells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;
        }
    }
}
