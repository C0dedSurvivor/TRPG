using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipInventory : GridInventoryGUI
{
    public GameObject equipButton;
    public GameObject discardButton;

    public GameObject item1Info;
    public GameObject item2Info;

    public GameObject unequipButton;

    //To hide the skill pieces
    public GameObject skillText;
    public GameObject skillButton;

    //Currently selected equipment slot
    private int invSlot;

    /// <summary>
    /// Opens the equippables viewer for a given slot
    /// </summary>
    /// <param name="inv">What filter to use</param>
    public void OpenEquipInventory(int inv)
    {
        invSlot = inv;
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


    //Needs a ground-up rewrite


    /// <summary>
    /// If the player is mousing over an equippable, displays the item info panel and updates the contained information accordingly
    /// </summary>
    /// <param name="item">The ID of the item being moused over, negative for an equipped item and positive for an unequipped item</param>
    public override void MouseOverItem(int item)
    {
        itemInfo.SetActive(true);
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);

        //If mousing over an equipped item
        if (item < 0)
        {
            if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1) != null)
            {

                UpdateItemInfo(item1Info, GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1).Name);
                item2Info.SetActive(false);
            }
            else
                itemInfo.SetActive(false);
        }
        //If the player is mousing over an unequipped item for a slot with nothing equipped
        else if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) == null)
        {
            UpdateItemInfo(item1Info, itemList[item].Name);
            item2Info.SetActive(false);
        }
        //If player is mousing over an unequipped item for an inventory slot with an equipped item
        else
        {
            string itemName = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot).Name;
            UpdateItemInfo(item1Info, itemName);
            UpdateItemInfo(item2Info, itemList[item].Name);

            //
            //
            //Sets the comparison colors for all of the stats
            //
            //

            EquippableBase equipped = ((EquippableBase)Registry.ItemRegistry[itemName]);
            EquippableBase inInv = ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]);
            Text[] children1 = item1Info.transform.GetComponentsInChildren<Text>();
            Text[] children2 = item2Info.transform.GetComponentsInChildren<Text>();
            
            UpdateComparison(equipped, inInv, Stats.MaxHealth, children1[2], children2[2]);
            UpdateComparison(equipped, inInv, Stats.Attack, children1[3], children2[3]);
            UpdateComparison(equipped, inInv, Stats.MagicAttack, children1[4], children2[4]);
            UpdateComparison(equipped, inInv, Stats.Defense, children1[5], children2[5]);
            UpdateComparison(equipped, inInv, Stats.MagicDefense, children1[6], children2[6]);
            UpdateComparison(equipped, inInv, Stats.CritChance, children1[7], children2[7]);
        }
        base.MouseOverItem(item);
    }

    private void UpdateItemInfo(GameObject itemViewer, string name)
    {
        itemViewer.SetActive(true);

        EquippableBase item = ((EquippableBase)Registry.ItemRegistry[name]);

        Text[] children = itemViewer.transform.GetComponentsInChildren<Text>();

        children[0].text = name;
        children[1].text = "Equipment type: ";
        switch (item.equipSlot)
        {
            case 0:
                children[1].text += "Weapon";
                break;
            case 1:
                children[1].text += "Helmet";
                break;
            case 2:
                children[1].text += "Chestplate";
                break;
            case 3:
                children[1].text += "Legs";
                break;
            case 4:
                children[1].text += "Boots";
                break;
            case 5:
                children[1].text += "Hands";
                break;
            case 6:
                children[1].text += "Accessory";
                break;
        }

        children[2].color = new Color(0.195f, 0.195f, 0.195f, 1);
        children[2].text = "Health: " + item.stats[Stats.MaxHealth];

        children[3].color = new Color(0.195f, 0.195f, 0.195f, 1);
        children[3].text = "Physical Strength: " + item.stats[Stats.Attack];

        children[4].color = new Color(0.195f, 0.195f, 0.195f, 1);
        children[4].text = "AEtheric Strength: " + item.stats[Stats.MagicAttack];

        children[5].color = new Color(0.195f, 0.195f, 0.195f, 1);
        children[5].text = "Physical Defense: " + item.stats[Stats.Defense];

        children[6].color = new Color(0.195f, 0.195f, 0.195f, 1);
        children[6].text = "AEtheric Defense: " + item.stats[Stats.MagicDefense];

        children[7].color = new Color(0.195f, 0.195f, 0.195f, 1);
        children[7].text = "Crit Chance: " + item.stats[Stats.CritChance] + "%";

        children[8].text = item.FlavorText;
        children[9].text = "Sells for: " + item.SellAmount;
    }

    private void UpdateComparison(EquippableBase equipped, EquippableBase inInv, Stats stat, Text equippedText, Text inInvText)
    {
        if (equipped.stats[stat] > inInv.stats[stat])
        {
            equippedText.color = Color.green;
            inInvText.color = Color.red;
        }
        else if (equipped.stats[stat] < inInv.stats[stat])
        {
            equippedText.color = Color.red;
            inInvText.color = Color.green;
        }
        else
        {
            equippedText.color = Color.blue;
            inInvText.color = Color.blue;
        }
    }
}
