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

    //to hide the skill pieces
    public GameObject skillText;
    public GameObject skillButton;

    //currently selected equipment slot
    private int invSlot;

    public void OpenEquipInventory(int inv)
    {
        invSlot = inv;
        //Min makes both accessory slots grab the accessory inventory
        itemList = Inventory.GetItemList(Mathf.Min(invSlot, 6));

        GenerateInventory();

        skillText.SetActive(false);
        skillButton.SetActive(false);

        if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) != null)
            unequipButton.SetActive(true);
        else
            unequipButton.SetActive(false);
    }

    public override void Close()
    {
        base.Close();
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        unequipButton.SetActive(false);
        skillText.SetActive(true);
        skillButton.SetActive(true);
    }

    public override void SelectItem(int item)
    {
        base.SelectItem(item);
        equipButton.SetActive(true);
        discardButton.SetActive(true);
    }

    public override void Discard()
    {
        base.Discard();
        Inventory.RemoveItem(itemList[selectedItem].Name, 1);
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        OpenEquipInventory(invSlot);
    }

    public void Equip()
    {
        int previousHealth = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].mHealth;
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot) != null)
            Inventory.AddItem(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].EquipItem(itemList[selectedItem].Name, invSlot), 1);
        else
            GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].EquipItem(itemList[selectedItem].Name, invSlot);
        Inventory.RemoveItem(itemList[selectedItem].Name, 1);
        Inventory.SortInventory((int)Inventory.sortingType);
        GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].CheckHealthChange(previousHealth);
        GetComponentInParent<PauseGUI>().UpdatePlayerEquipped();
        selectedItem = -1;
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        OpenEquipInventory(invSlot);
    }

    public void Unequip()
    {
        int previousHealth = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].mHealth;
        Inventory.AddItem(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].EquipItem(null, invSlot), 1);
        Inventory.SortInventory((int)Inventory.sortingType);
        Debug.Log(GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot));
        GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].CheckHealthChange(previousHealth);
        GetComponentInParent<PauseGUI>().UpdatePlayerEquipped();
        selectedItem = -1;
        equipButton.SetActive(false);
        discardButton.SetActive(false);
        unequipButton.SetActive(false);
        OpenEquipInventory(invSlot);
    }

    public override void MouseOverItem(int item)
    {
        itemInfo.SetActive(true);
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        //if (itemInfo.transform.localPosition.y < 0 && Mathf.Abs(itemInfo.transform.localPosition.y) + itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight > Screen.height / 2)
        //    itemInfo.transform.position = new Vector3(itemInfo.transform.position.x, Screen.height / 2 - itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight / 2, itemInfo.transform.position.z);

        //if mousing over an equipped item
        if (item < 0)
        {
            if (GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1) != null)
            {
                string itemName = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(-item - 1);
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
                item1Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemName]).health;

                item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(3).GetComponent<Text>().text = "";
                if (((EquippableBase)Registry.ItemRegistry[itemName]).statType == 0)
                    item1Info.transform.GetChild(3).GetComponent<Text>().text += "Physical";
                else
                    item1Info.transform.GetChild(3).GetComponent<Text>().text += "AEtheric";
                item1Info.transform.GetChild(3).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemName]).strength;

                item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(4).GetComponent<Text>().text = "";
                if (((EquippableBase)Registry.ItemRegistry[itemName]).statType == 0)
                    item1Info.transform.GetChild(4).GetComponent<Text>().text += "Physical";
                else
                    item1Info.transform.GetChild(4).GetComponent<Text>().text += "AEtheric";
                item1Info.transform.GetChild(4).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemName]).defense;

                item1Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item1Info.transform.GetChild(5).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemName]).critChanceMod + "%";

                item1Info.transform.GetChild(6).GetComponent<Text>().text = Registry.ItemRegistry[itemName].FlavorText;
                item1Info.transform.GetChild(7).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemName].SellAmount;
                item2Info.SetActive(false);
            }
            else
                itemInfo.SetActive(false);
        }
        //if the player is mousing over an unequipped item for a slot with nothing equipped
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
            item1Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health;

            item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(3).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                item1Info.transform.GetChild(3).GetComponent<Text>().text += "Physical";
            else
                item1Info.transform.GetChild(3).GetComponent<Text>().text += "AEtheric";
            item1Info.transform.GetChild(3).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength;

            item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(4).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                item1Info.transform.GetChild(4).GetComponent<Text>().text += "Physical";
            else
                item1Info.transform.GetChild(4).GetComponent<Text>().text += "AEtheric";
            item1Info.transform.GetChild(4).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense;

            item1Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(5).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod + "%";

            item1Info.transform.GetChild(6).GetComponent<Text>().text = Registry.ItemRegistry[itemList[item].Name].FlavorText;
            item1Info.transform.GetChild(7).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;
            item2Info.SetActive(false);
        }
        //if player is mousing over an unequipped item for an inventory with an equipped item
        else
        {
            string itemName = GameStorage.playerMasterList[GameStorage.activePlayerList[PauseGUI.playerID]].GetEquipped(invSlot);
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
            item1Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemName]).health;

            item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(3).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemName]).statType == 0)
                item1Info.transform.GetChild(3).GetComponent<Text>().text += "Physical";
            else
                item1Info.transform.GetChild(3).GetComponent<Text>().text += "AEtheric";
            item1Info.transform.GetChild(3).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemName]).strength;

            item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(4).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemName]).statType == 0)
                item1Info.transform.GetChild(4).GetComponent<Text>().text += "Physical";
            else
                item1Info.transform.GetChild(4).GetComponent<Text>().text += "AEtheric";
            item1Info.transform.GetChild(4).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemName]).defense;

            item1Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item1Info.transform.GetChild(5).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemName]).critChanceMod + "%";

            item1Info.transform.GetChild(6).GetComponent<Text>().text = Registry.ItemRegistry[itemName].FlavorText;
            item1Info.transform.GetChild(7).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemName].SellAmount;

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
            item2Info.transform.GetChild(2).GetComponent<Text>().text = "Health: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health;

            item2Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(3).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                item2Info.transform.GetChild(3).GetComponent<Text>().text += "Physical";
            else
                item2Info.transform.GetChild(3).GetComponent<Text>().text += "AEtheric";
            item2Info.transform.GetChild(3).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength;

            item2Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(4).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                item2Info.transform.GetChild(4).GetComponent<Text>().text += "Physical";
            else
                item2Info.transform.GetChild(4).GetComponent<Text>().text += "AEtheric";
            item2Info.transform.GetChild(4).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense;

            item2Info.transform.GetChild(5).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            item2Info.transform.GetChild(5).GetComponent<Text>().text = "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod + "%";

            item2Info.transform.GetChild(6).GetComponent<Text>().text = Registry.ItemRegistry[itemList[item].Name].FlavorText;
            item2Info.transform.GetChild(7).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;

            //sets the comparison colors
            //health
            if (((EquippableBase)Registry.ItemRegistry[itemName]).health > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health)
            {
                item1Info.transform.GetChild(2).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(2).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).health < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health)
            {
                item1Info.transform.GetChild(2).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(2).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(2).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(2).GetComponent<Text>().color = Color.blue;
            }

            //strength
            if (((EquippableBase)Registry.ItemRegistry[itemName]).statType == ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType)
            {
                if (((EquippableBase)Registry.ItemRegistry[itemName]).strength > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength)
                {
                    item1Info.transform.GetChild(3).GetComponent<Text>().color = Color.green;
                    item2Info.transform.GetChild(3).GetComponent<Text>().color = Color.red;
                }
                else if (((EquippableBase)Registry.ItemRegistry[itemName]).strength < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength)
                {
                    item1Info.transform.GetChild(3).GetComponent<Text>().color = Color.red;
                    item2Info.transform.GetChild(3).GetComponent<Text>().color = Color.green;
                }
                else
                {
                    item1Info.transform.GetChild(3).GetComponent<Text>().color = Color.blue;
                    item2Info.transform.GetChild(3).GetComponent<Text>().color = Color.blue;
                }
            }
            else
            {
                item1Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item2Info.transform.GetChild(3).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            }

            //defense
            if (((EquippableBase)Registry.ItemRegistry[itemName]).statType == ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType)
            {
                if (((EquippableBase)Registry.ItemRegistry[itemName]).defense > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense)
                {
                    item1Info.transform.GetChild(4).GetComponent<Text>().color = Color.green;
                    item2Info.transform.GetChild(4).GetComponent<Text>().color = Color.red;
                }
                else if (((EquippableBase)Registry.ItemRegistry[itemName]).defense < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense)
                {
                    item1Info.transform.GetChild(4).GetComponent<Text>().color = Color.red;
                    item2Info.transform.GetChild(4).GetComponent<Text>().color = Color.green;
                }
                else
                {
                    item1Info.transform.GetChild(4).GetComponent<Text>().color = Color.blue;
                    item2Info.transform.GetChild(4).GetComponent<Text>().color = Color.blue;
                }
            }
            else
            {
                item1Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
                item2Info.transform.GetChild(4).GetComponent<Text>().color = new Color(0.195f, 0.195f, 0.195f, 1);
            }

            //crit chance
            if (((EquippableBase)Registry.ItemRegistry[itemName]).critChanceMod > ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod)
            {
                item1Info.transform.GetChild(5).GetComponent<Text>().color = Color.green;
                item2Info.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            }
            else if (((EquippableBase)Registry.ItemRegistry[itemName]).critChanceMod < ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod)
            {
                item1Info.transform.GetChild(5).GetComponent<Text>().color = Color.red;
                item2Info.transform.GetChild(5).GetComponent<Text>().color = Color.green;
            }
            else
            {
                item1Info.transform.GetChild(5).GetComponent<Text>().color = Color.blue;
                item2Info.transform.GetChild(5).GetComponent<Text>().color = Color.blue;
            }
        }
    }
}
