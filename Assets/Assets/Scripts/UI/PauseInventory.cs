using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseInventory : GearInventoryGUI {

    public Dropdown sorting;
    public Dropdown filter;
    public Button sortAndFilter;
    public Dropdown amtToDiscard;
    public Button discard;

    public override void GenerateInventory()
    {
        itemList = Inventory.GetItemList(filter.value - 1);
        base.GenerateInventory();
        sorting.gameObject.SetActive(true);
        filter.gameObject.SetActive(true);
        sortAndFilter.gameObject.SetActive(true);
    }

    public override void Destroy()
    {
        base.Destroy();
        sorting.gameObject.SetActive(false);
        filter.gameObject.SetActive(false);
        sortAndFilter.gameObject.SetActive(false);
    }

    public override void MoveToButton(int button)
    {
        base.MoveToButton(button);
        amtToDiscard.ClearOptions();
        if (Registry.ItemRegistry[itemList[selectedItem].Name] is EquippableBase)
        {
            //doesn't matter what's in there, just that there is something there
            amtToDiscard.AddOptions(new List<Dropdown.OptionData>() { new Dropdown.OptionData("Yes") });
            amtToDiscard.gameObject.SetActive(false);
        }
        else
        {
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

    public override void Discard()
    {
        itemList[selectedItem].amount -= amtToDiscard.value + 1;
        Inventory.RemoveItem(itemList[selectedItem].Name, amtToDiscard.value + 1);
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
            itemBoxList[selectedItem].GetComponent<InventoryItemButton>().UpdateItem(itemList[selectedItem]);
            amtToDiscard.ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for (int i = 1; i <= itemList[selectedItem].amount; i++)
            {
                options.Add(new Dropdown.OptionData("" + i));
            }
            amtToDiscard.AddOptions(options);
            amtToDiscard.value = Mathf.Min(itemList[selectedItem].amount - 1, amtToDiscard.value);
        }
    }

    public void SortAndChangeFilter()
    {
        Inventory.SortInventory(sorting.value);
        ChangeInventory(filter.value - 1);
    }

    public override void MouseOverItem(int item)
    {
        itemInfo.SetActive(true);
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        //if (itemInfo.transform.localPosition.y < 0 && Mathf.Abs(itemInfo.transform.localPosition.y) + itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight > Screen.height / 2)
        //    itemInfo.transform.position = new Vector3(itemInfo.transform.position.x, Screen.height / 2 - itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight / 2, itemInfo.transform.position.z);
        itemInfo.transform.GetChild(0).GetComponent<Text>().text = itemList[item].Name;
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
            itemInfo.transform.GetChild(2).GetComponent<Text>().text = "";
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health != 0)
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Health: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health;
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength != 0)
            {
                if (itemInfo.transform.GetChild(2).GetComponent<Text>().text != "")
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "\n";
                if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Physical";
                else
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "AEtheric";
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength;
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense != 0)
            {
                if (itemInfo.transform.GetChild(2).GetComponent<Text>().text != "")
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "\n";
                if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).statType == 0)
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Physical";
                else
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "AEtheric";
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense;
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod != 0)
            {
                if (itemInfo.transform.GetChild(2).GetComponent<Text>().text != "")
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "\n";
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod + "%";
            }
            itemInfo.transform.GetChild(3).GetComponent<Text>().text = Registry.ItemRegistry[itemList[item].Name].FlavorText;
            itemInfo.transform.GetChild(4).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;
        }
        else
        {
            itemInfo.transform.GetChild(1).GetComponent<Text>().text = itemList[item].amount + "/" + Registry.ItemRegistry[itemList[item].Name].MaxStack;
            itemInfo.transform.GetChild(2).GetComponent<Text>().text = Registry.ItemRegistry[itemList[item].Name].FlavorText;
            itemInfo.transform.GetChild(3).GetComponent<Text>().text = "Sells for: " + Registry.ItemRegistry[itemList[item].Name].SellAmount;
            itemInfo.transform.GetChild(4).GetComponent<Text>().text = "";
        }
    }
}
