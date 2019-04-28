using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipInventory : GridInventoryGUI
{
    public GameObject equipButton;
    public GameObject discardButton;

    public GameObject unequipButton;

    //To hide the skill pieces
    public GameObject skillText;
    public GameObject skillButton;

    public GameObject itemInfoTextPrefab;

    //Currently selected equipment slot
    private int invSlot;

    private int mousedOverItem;

    /// <summary>
    /// Opens the equippables viewer for a given slot
    /// </summary>
    /// <param name="inv">What filter to use</param>
    public void OpenEquipInventory(int inv)
    {
        invSlot = inv;
        mousedOverItem = int.MinValue;
        //Min makes both accessory slots grab the accessory inventory
        itemList = Inventory.GetItemList(Mathf.Min(invSlot, 6));

        GenerateInventory();

        Debug.Log("Disabling skill stuff");
        skillText.SetActive(false);
        skillButton.SetActive(false);

        if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) != null)
            unequipButton.SetActive(true);
        else
            unequipButton.SetActive(false);
    }

    /// <summary>
    /// Closes the inventory viewer, hides the inventory options and shows the skill options
    /// </summary>
    public override void Close()
    {
        base.Close();
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        unequipButton.SetActive(false);
        skillText.SetActive(true);
        skillButton.SetActive(true);
        mousedOverItem = int.MinValue;
    }

    /// <summary>
    /// When a player selects an item from the inventory
    /// </summary>
    /// <param name="item">Item they selected</param>
    public override void SelectItem(int item)
    {
        base.SelectItem(item);
        equipButton.SetActive(true);
        discardButton.SetActive(true);
    }

    /// <summary>
    /// Discards an item from the inventory
    /// </summary>
    public override void Discard()
    {
        //Removes the item from the inventory
        Inventory.RemoveItem(itemList[selectedItem].Name, 1);
        base.Discard();
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        //Refreshes the inventory viewer
        OpenEquipInventory(invSlot);
    }

    /// <summary>
    /// Equips an item from the inventory, placing the old item back into the inventory
    /// </summary>
    public void Equip()
    {
        //Grabs the current max health of the player in case it changes after the item is equipped
        int previousHealth = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEffectiveStat(Stats.MaxHealth);
        //Equip the new item. If the slot already has something equipped, put that item back into the inventory
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) != null)
            Inventory.AddItem(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].EquipItem(itemList[selectedItem] as Equippable, invSlot));
        else
            GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].EquipItem(itemList[selectedItem] as Equippable, invSlot);
        //Removes the item to be equipped from the inventory and sorts it
        Inventory.RemoveItem(itemList[selectedItem].Name, 1);
        Inventory.SortInventory((int)Inventory.sortingType);
        //Checks to see if max health was affected and modifies current health accordingly
        GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].CheckHealthChange(previousHealth);
        //Updates the equipped item and stat display for the player
        GetComponentInParent<PauseGUI>().UpdatePlayerEquipped();
        selectedItem = -1;
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        //Refreshes the inventory viewer
        OpenEquipInventory(invSlot);
    }

    /// <summary>
    /// Unequips and item from the player, placing it back in the inventory
    /// </summary>
    public void Unequip()
    {
        //Grabs the current max health of the player in case it changes after the item is de-equipped
        int previousHealth = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEffectiveStat(Stats.MaxHealth);
        //Adds the item back to the inventory and sorts it
        Inventory.AddItem(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].EquipItem(null, invSlot));
        Inventory.SortInventory((int)Inventory.sortingType);
        //Debug.Log(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot));
        //Checks to see if max health was affected and modifies current health accordingly
        GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].CheckHealthChange(previousHealth);
        //Updates the equipped item and stat display for the player
        GetComponentInParent<PauseGUI>().UpdatePlayerEquipped();
        selectedItem = -1;
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        unequipButton.SetActive(false);
        //Refreshes the inventory viewer
        OpenEquipInventory(invSlot);
    }

    /// <summary>
    /// If the player is mousing over an equippable, displays the item info panel and updates the contained information accordingly
    /// </summary>
    /// <param name="item">The ID of the item being moused over, negative for an equipped item and positive for an unequipped item</param>
    public override void MouseOverItem(int item)
    {
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        if (item != mousedOverItem)
        {
            itemInfo.SetActive(true);
            mousedOverItem = item;
            //If mousing over an equipped item
            if (item < 0)
            {
                if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1) != null)
                    UpdateSingleItemInfo(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1).Name);
                else
                    itemInfo.SetActive(false);
            }
            //If the player is mousing over an unequipped item for a slot with nothing equipped
            else if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) == null)
            {
                UpdateSingleItemInfo(itemList[item].Name);
            }
            //If player is mousing over an unequipped item for an inventory slot with an equipped item
            else
            {
                string itemName = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot).Name;
                UpdateComparison(((EquippableBase)Registry.ItemRegistry[itemName]), itemList[item].Name);
            }
        }
        base.MouseOverItem(item);
    }

    /// <summary>
    /// Resets the moused over item when the item is no longer moused over
    /// </summary>
    public override void MouseLeaveItem()
    {
        mousedOverItem = int.MinValue;
        base.MouseLeaveItem();
    }

    /// <summary>
    /// Updates the item info with the base info for the given item
    /// </summary>
    /// <param name="textList">The text items to update with the info</param>
    /// <param name="item">The name of the item</param>
    private void UpdateBaseInfo(Text[] textList, string item)
    {
        textList[0].text = item;
        textList[1].text = "Equipment type: ";
        switch (((EquippableBase)Registry.ItemRegistry[item]).equipSlot)
        {
            case 0:
                textList[1].text += "Weapon";
                break;
            case 1:
                textList[1].text += "Helmet";
                break;
            case 2:
                textList[1].text += "Chestplate";
                break;
            case 3:
                textList[1].text += "Legs";
                break;
            case 4:
                textList[1].text += "Boots";
                break;
            case 5:
                textList[1].text += "Hands";
                break;
            case 6:
                textList[1].text += "Accessory";
                break;
        }
    }

    /// <summary>
    /// Updates the info panel with the info of a given item
    /// </summary>
    /// <param name="name">The name of the item</param>
    private void UpdateSingleItemInfo(string name)
    {
        EquippableBase item = ((EquippableBase)Registry.ItemRegistry[name]);

        Text[] children = itemInfo.transform.GetComponentsInChildren<Text>();

        UpdateBaseInfo(children, name);

        //Removes the previous info texts
        for(int i = 2; i < children.Length; i++)
        {
            Destroy(itemInfo.transform.GetChild(i).gameObject);
        }

        foreach(Stats stat in (Stats[])Enum.GetValues(typeof(Stats)))
        {
            if (item.stats.ContainsKey(stat))
            {
                Text text = Instantiate(itemInfoTextPrefab, itemInfo.transform).GetComponent<Text>();
                string statName = GameStorage.StatToString(stat);
                bool isPercent = statName.Contains("Effectiveness") || statName.Contains("Receptiveness") || statName.Contains("Lifesteal") || statName.Contains("Chance");
                text.text = statName + ": " + ((isPercent && item.stats[stat] > 0) ? "+" : "") + item.stats[stat] + (isPercent ? "%" : "");
            }
        }

        Text flavorText = Instantiate(itemInfoTextPrefab, itemInfo.transform).GetComponent<Text>();
        flavorText.text = item.FlavorText;

        Text sellText = Instantiate(itemInfoTextPrefab, itemInfo.transform).GetComponent<Text>();
        sellText.text = "Sells for: " + item.SellAmount;
    }

    /// <summary>
    /// Updates the info panel to show a comparison between the equipped itm for a slot and the unequipped one
    /// </summary>
    /// <param name="equipped">The equipped item</param>
    /// <param name="inInvName">Name of the unequipped item</param>
    private void UpdateComparison(EquippableBase equipped, string inInvName)
    {
        Text[] children = itemInfo.transform.GetComponentsInChildren<Text>();
        
        UpdateBaseInfo(children, inInvName);

        EquippableBase inInv = ((EquippableBase)Registry.ItemRegistry[inInvName]);

        //Removes the previous info texts
        for (int i = 2; i < children.Length; i++)
        {
            Destroy(itemInfo.transform.GetChild(i).gameObject);
        }

        foreach (Stats stat in (Stats[])Enum.GetValues(typeof(Stats)))
        {
            if (equipped.stats.ContainsKey(stat) || inInv.stats.ContainsKey(stat))
            {
                Text text = Instantiate(itemInfoTextPrefab, itemInfo.transform).GetComponent<Text>();
                string statName = GameStorage.StatToString(stat);
                bool isPercent = statName.Contains("Effectiveness") || statName.Contains("Receptiveness") || statName.Contains("Lifesteal") || statName.Contains("Chance");
                int equippedStat = equipped.stats.ContainsKey(stat) ? equipped.stats[stat] : 0;
                int inInvStat = inInv.stats.ContainsKey(stat) ? inInv.stats[stat] : 0;
                text.text = statName + ": " + ((isPercent && equippedStat > 0) ? "+" : "") + equippedStat + (isPercent ? "%" : "") + " -> " + ((isPercent && inInvStat > 0) ? "+" : "") + inInvStat + (isPercent ? "%" : "");
            }
        }

        Text flavorText = Instantiate(itemInfoTextPrefab, itemInfo.transform).GetComponent<Text>();
        flavorText.text = inInv.FlavorText;

        Text sellText = Instantiate(itemInfoTextPrefab, itemInfo.transform).GetComponent<Text>();
        sellText.text = "Sells for: " + inInv.SellAmount;
    }
}
