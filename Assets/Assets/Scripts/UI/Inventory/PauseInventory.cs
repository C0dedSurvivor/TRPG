using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseInventory : GearInventoryGUI
{
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
        amtToDiscard.gameObject.SetActive(false);
        discard.gameObject.SetActive(false);
    }

    /// <summary>
    /// Clears all of the visibles and data
    /// </summary>
    public override void Close()
    {
        base.Close();
        sorting.gameObject.SetActive(false);
        filter.gameObject.SetActive(false);
        sortAndFilter.gameObject.SetActive(false);
        amtToDiscard.gameObject.SetActive(false);
        discard.gameObject.SetActive(false);
    }

    /// <summary>
    /// Rotates so the selected button is at the center of the wheel
    /// </summary>
    /// <param name="button">The ID of the button to center</param>
    public override void MoveToButton(int button)
    {
        base.MoveToButton(button);
        sorting.gameObject.SetActive(false);
        filter.gameObject.SetActive(false);
        sortAndFilter.gameObject.SetActive(false);
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
            sorting.gameObject.SetActive(true);
            filter.gameObject.SetActive(true);
            sortAndFilter.gameObject.SetActive(true);
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
        Inventory.SortInventory((SortingType)sorting.value);
        ChangeInventory(filter.value - 1);
    }

    /// <summary>
    /// Displays and updates the item info card if the player is mousing over an item
    /// </summary>
    /// <param name="index">The index of the item you are mousing over</param>
    public override void MouseOverItem(int index)
    {
        Text[] children = itemInfo.transform.GetComponentsInChildren<Text>();

        itemInfo.SetActive(true);
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        itemInfo.transform.GetChild(0).GetComponent<Text>().text = itemList[index].Name;
        //Has to display extra stat information if the item is an equippable
        if (Registry.ItemRegistry[itemList[index].Name] is EquippableBase)
        {
            EquippableBase item = ((EquippableBase)Registry.ItemRegistry[itemList[index].Name]);
            children[1].text = "Equipment type: " + item.equipSlot.ToString();
            foreach (Stats stat in (Stats[])Enum.GetValues(typeof(Stats)))
            {
                if (item.stats.ContainsKey(stat))
                {
                    string statName = GameStorage.StatToString(stat);
                    bool isPercent = statName.Contains("Effectiveness") || statName.Contains("Receptiveness") || statName.Contains("Lifesteal") || statName.Contains("Chance");
                    children[1].text += "\n" + statName + ": " + item.stats[stat] + (isPercent ? "%" : "");
                }
            }
        }
        else
        {
            children[1].text = itemList[index].amount + "/" + Registry.ItemRegistry[itemList[index].Name].MaxStack;
        }
        children[1].text += "\n" + Registry.ItemRegistry[itemList[index].Name].FlavorText;
        children[1].text += "\nSells for: " + Registry.ItemRegistry[itemList[index].Name].SellAmount;
        base.MouseOverItem(index);
    }
}
