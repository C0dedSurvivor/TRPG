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
                string itemName = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1).Name;
                item1Info.transform.GetChild(0).GetComponent<Text>().text = itemName;
                item1Info.transform.GetChild(1).GetComponent<Text>().text = "Equipment type: ";
                switch (((EquippableBase)Registry.ItemRegistry[itemName]).equipSlot)
                {
                    case 0:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Weapon";
                        break;
                    case 1:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Helmet";
                        break;
                    case 2:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Chestplate";
                        break;
                    case 3:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Legs";
                        break;
                    case 4:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Boots";
                        break;
                    case 5:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Hands";
                        break;
                    case 6:
                        item1Info.transform.GetChild(1).GetComponent<Text>().text += "Accessory";
                        break;
                }

                item1Info.transform.GetChild(2).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MaxHealth];

                item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(3).GetComponent<Text>().text = "Physical Strength: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Attack];

                item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(4).GetComponent<Text>().text = "AEtheric Strength: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicAttack];

                item1Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(5).GetComponent<Text>().text = "Physical Defense: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Defense];

                item1Info.transform.GetChild(6).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(6).GetComponent<Text>().text = "AEtheric Defense: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicDefense];

                item1Info.transform.GetChild(7).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(7).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.CritChance] + "%";

                item1Info.transform.GetChild(8).GetComponent<Text>().text = Registry.ItemRegistry[itemName].FlavorText;
                item1Info.transform.GetChild(9).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemName].SellAmount;

                item2Info.SetActive(false);
            }
            else
                itemInfo.SetActive(false);
        }
        //If the player is mousing over an unequipped item for a slot with nothing equipped
        else if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) == null)
        {
            item1Info.transform.GetChild(0).GetComponent<Text>().text = itemList[item].Name;
            item1Info.transform.GetChild(1).GetComponent<Text>().text = "Equipment type: ";
            switch (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).equipSlot)
            {
                case 0:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Weapon";
                    break;
                case 1:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Helmet";
                    break;
                case 2:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Chestplate";
                    break;
                case 3:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Legs";
                    break;
                case 4:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Boots";
                    break;
                case 5:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Hands";
                    break;
                case 6:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Accessory";
                    break;
            }

            item1Info.transform.GetChild(2).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MaxHealth];

            item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(3).GetComponent<Text>().text = "Physical Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Attack];

            item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(4).GetComponent<Text>().text = "AEtheric Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicAttack];

            item1Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(5).GetComponent<Text>().text = "Physical Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Defense];

            item1Info.transform.GetChild(6).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(6).GetComponent<Text>().text = "AEtheric Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicDefense];

            item1Info.transform.GetChild(7).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(7).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.CritChance] + "%";

            item1Info.transform.GetChild(8).GetComponent<Text>().text = Registry.ItemRegistry[itemList[item].Name].FlavorText;
            item1Info.transform.GetChild(9).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;
            item2Info.SetActive(false);
        }
        //If player is mousing over an unequipped item for an inventory slot with an equipped item
        else
        {
            string itemName = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot).Name;
            item1Info.transform.GetChild(0).GetComponent<Text>().text = itemName;
            item1Info.transform.GetChild(1).GetComponent<Text>().text = "Equipment type: ";
            switch (((EquippableBase)Registry.ItemRegistry[itemName]).equipSlot)
            {
                case 0:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Weapon";
                    break;
                case 1:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Helmet";
                    break;
                case 2:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Chestplate";
                    break;
                case 3:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Legs";
                    break;
                case 4:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Boots";
                    break;
                case 5:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Hands";
                    break;
                case 6:
                    item1Info.transform.GetChild(1).GetComponent<Text>().text += "Accessory";
                    break;
            }

            item1Info.transform.GetChild(2).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MaxHealth];

            item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(3).GetComponent<Text>().text = "Physical Strength: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Attack];

            item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(4).GetComponent<Text>().text = "AEtheric Strength: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicAttack];

            item1Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(5).GetComponent<Text>().text = "Physical Defense: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Defense];

            item1Info.transform.GetChild(6).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(6).GetComponent<Text>().text = "AEtheric Defense: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicDefense];

            item1Info.transform.GetChild(7).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(7).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.CritChance] + "%";

            item1Info.transform.GetChild(8).GetComponent<Text>().text = Registry.ItemRegistry[itemName].FlavorText;
            item1Info.transform.GetChild(9).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemName].SellAmount;

            item2Info.SetActive(true);

            item2Info.transform.GetChild(0).GetComponent<Text>().text = itemList[item].Name;
            item2Info.transform.GetChild(1).GetComponent<Text>().text = "Equipment type: ";
            switch (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).equipSlot)
            {
                case 0:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Weapon";
                    break;
                case 1:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Helmet";
                    break;
                case 2:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Chestplate";
                    break;
                case 3:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Legs";
                    break;
                case 4:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Boots";
                    break;
                case 5:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Hands";
                    break;
                case 6:
                    item2Info.transform.GetChild(1).GetComponent<Text>().text += "Accessory";
                    break;
            }

            item2Info.transform.GetChild(2).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MaxHealth];

            item2Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(3).GetComponent<Text>().text = "Physical Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Attack];

            item2Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(4).GetComponent<Text>().text = "AEtheric Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicAttack];

            item2Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(5).GetComponent<Text>().text = "Physical Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Defense];

            item2Info.transform.GetChild(6).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(6).GetComponent<Text>().text = "AEtheric Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicDefense];

            item2Info.transform.GetChild(7).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(7).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.CritChance] + "%";

            item2Info.transform.GetChild(8).GetComponent<Text>().text = Registry.ItemRegistry[itemList[item].Name].FlavorText;
            item2Info.transform.GetChild(9).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;

            //
            //
            //Sets the comparison colors for all of the stats
            //
            //

            //Health
            if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MaxHealth] > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MaxHealth])
            {
                item1Info.transform.GetChild(2).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MaxHealth] < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MaxHealth])
            {
                item1Info.transform.GetChild(2).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(2).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(2).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(2).GetComponent<Text>().color = Color.blue;
            }

            //Strength
            if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Attack] > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Attack])
            {
                item1Info.transform.GetChild(3).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(3).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Attack] < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Attack])
            {
                item1Info.transform.GetChild(3).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(3).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(3).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(3).GetComponent<Text>().color = Color.blue;
            }

            //aEtheric Strength
            if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicAttack] > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicAttack])
            {
                item1Info.transform.GetChild(4).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(4).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicAttack] < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicAttack])
            {
                item1Info.transform.GetChild(4).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(4).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(4).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(4).GetComponent<Text>().color = Color.blue;
            }

            //Defense
            if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Defense] > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Defense])
            {
                item1Info.transform.GetChild(5).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.Defense] < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.Defense])
            {
                item1Info.transform.GetChild(5).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(5).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(5).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(5).GetComponent<Text>().color = Color.blue;
            }

            //aEtheric Defense
            if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicDefense] > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicDefense])
            {
                item1Info.transform.GetChild(6).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(6).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.MagicDefense] < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.MagicDefense])
            {
                item1Info.transform.GetChild(6).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(6).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(6).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(6).GetComponent<Text>().color = Color.blue;
            }

            //Crit chance
            if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.CritChance] > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.CritChance])
            {
                item1Info.transform.GetChild(7).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(7).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).stats[Stats.CritChance] < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).stats[Stats.CritChance])
            {
                item1Info.transform.GetChild(7).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(7).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(7).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(7).GetComponent<Text>().color = Color.blue;
            }
        }
        base.MouseOverItem(item);
    }
}
