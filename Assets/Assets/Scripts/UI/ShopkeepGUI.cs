using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopkeepGUI : GearInventoryGUI
{
    public GameObject window;
    public Dropdown sorting;
    public Dropdown filter;
    public GameObject confirmButton;

    public Dropdown amt;
    public GameObject closeWindow;

    public List<int> selected = new List<int>();

    public string form = "buy";

    //0 = all, 1 = weapons, 2 = battle items, 3 = materials
    private int currentFilter = 0;

    public void OpenBuyingShop(List<string> shopList)
    {
        form = "buy";
        Destroy();
        foreach (string i in shopList)
        {
            switch (currentFilter)
            {
                //all
                case 0:
                    itemList.Add(new StoredItem(i, 1));
                    break;
                //all equippables
                case 1:
                    if (Registry.ItemRegistry[i] is EquippableBase)
                        itemList.Add(new StoredItem(i, 1));
                    break;
                //all battle items
                case 2:
                    if (Registry.ItemRegistry[i] is BattleItemBase)
                        itemList.Add(new StoredItem(i, 1));
                    break;
                //all materials
                case 3:
                    if (!(Registry.ItemRegistry[i] is EquippableBase || Registry.ItemRegistry[i] is BattleItemBase))
                        itemList.Add(new StoredItem(i, 1));
                    break;
            }
        }
        SortShop();
    }

    public void OpenSellingShop()
    {
        form = "sell";
        Destroy();
    }

    public override void Destroy()
    {
        base.Destroy();
        selected.Clear();
    }

    public void SortShop()
    {
        //does the initial sorting
        itemList.Sort((x, y) => x.CompareTo(y));

        //sorts unbuyable items to the bottom and disables it
        int place = 0;
        for (int i = 0; i < itemList.Count; i++)
        {
            if (GetCost(itemList[i].Name) > GameStorage.playerCurrency || (!(Registry.ItemRegistry[itemList[i].Name] is EquippableBase) && Inventory.GetItemAmount(itemList[i].Name) >= Registry.ItemRegistry[itemList[i].Name].MaxStack))
            {
                itemList[place].amount = 0;
                itemList.Insert(itemList.Count - 1, itemList[place]);
            }
            else
            {
                itemList[place].amount = 1;
                place++;
            }
        }

        Destroy();
        GenerateInventory();

        for (int i = 0; i < itemBoxList.Count; i++)
        {
            if (itemList[i].amount == 0)
            {
                itemBoxList[i].GetComponent<Button>().interactable = false;
                itemBoxList[i].GetComponent<Image>().color = Color.red;
            }
            itemBoxList[i].GetComponent<Text>().text = "$" + GetCost(itemList[i].Name);
        }
    }

    public void Buy()
    {

    }

    public void Sell()
    {
        if (selected.Count == 1)
        {
            GameStorage.playerCurrency += (amt.value + 1) * Registry.ItemRegistry[itemList[selected[0]].Name].SellAmount;
            itemList[selected[0]].amount -= amt.value + 1;
            Inventory.RemoveItem(itemList[selectedItem].Name, amt.value + 1);
            if (itemList[selected[0]].amount == 0)
            {
                amt.gameObject.SetActive(false);
                amt.value = 0;
                confirmButton.gameObject.SetActive(false);


                //reset the shop


                if (itemList.Count >= selectedItem + 1)
                    MoveToButton(selectedItem + 1);
                else if (itemList.Count > 0)
                    MoveToButton(itemList.Count);
            }
            else
            {
                itemBoxList[selectedItem].GetComponent<InventoryItemButton>().UpdateItem(itemList[selectedItem]);
                amt.ClearOptions();
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                for (int i = 1; i <= itemList[selectedItem].amount; i++)
                {
                    options.Add(new Dropdown.OptionData("" + i));
                }
                amt.AddOptions(options);
                amt.value = Mathf.Min(itemList[selectedItem].amount - 1, amt.value);
            }
        }
        else
        {
            foreach (int i in selected)
            {
                GameStorage.playerCurrency += itemList[i].amount * Registry.ItemRegistry[itemList[selected[0]].Name].SellAmount;
                Inventory.RemoveItem(itemList[i].Name, itemList[i].amount);
                itemList[i].amount = 0;
            }
            amt.gameObject.SetActive(false);
            amt.value = 0;
            confirmButton.gameObject.SetActive(false);
            selected.Clear();
        }

        if (itemList[selected[0]].amount == 0)
        {
            //SortAndChangeFilter();
            if (itemList.Count >= selectedItem + 1)
                MoveToButton(selectedItem + 1);
            else if (itemList.Count > 0)
                MoveToButton(itemList.Count);
            selectedItem = -1;
        }
        else
        {
        }
    }

    //is effectively OnClick as well
    public override void MoveToButton(int button)
    {
        base.MoveToButton(button);
        confirmButton.SetActive(true);
        //if player is buying
        if (form == "buy")
        {
            confirmButton.GetComponent<Text>().text = "Buy";
            if (Registry.ItemRegistry[itemList[selectedItem].Name] is EquippableBase)
            {
                amt.value = 0;
                amt.gameObject.SetActive(false);
            }
            else
            {
                amt.ClearOptions();
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                for (int i = 1; i <= Mathf.Min(Registry.ItemRegistry[itemList[selectedItem].Name].MaxStack - Inventory.GetItemAmount(itemList[selectedItem].Name), GameStorage.playerCurrency / GetCost(itemList[selectedItem].Name)); i++)
                {
                    options.Add(new Dropdown.OptionData("" + i));
                }
                amt.AddOptions(options);
                amt.value = 0;
                amt.gameObject.SetActive(true);
            }
        }
        //if player is selling
        else
        {
            confirmButton.GetComponent<Text>().text = "Sell";
            if (selected.Contains(selectedItem))
            {
                selected.Remove(selectedItem);
            }
            else
            {
                selected.Add(selectedItem);
            }
            if(selected.Count == 1)
            {
                amt.ClearOptions();
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                for (int i = 1; i <= Inventory.GetItemAmount(itemList[selectedItem].Name); i++)
                {
                    options.Add(new Dropdown.OptionData("" + i));
                }
                amt.AddOptions(options);
                amt.value = 0;
                amt.gameObject.SetActive(true);
            }
            else
            {
                amt.value = 0;
                amt.gameObject.SetActive(false);
            }
        }
    }

    public override void MouseOverItem(int item)
    {

    }

    private int GetCost(string i)
    {
        return Registry.ItemRegistry[i].SellAmount * 3;
    }
}
