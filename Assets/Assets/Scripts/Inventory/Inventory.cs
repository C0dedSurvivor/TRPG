using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum SortingType
{
    //starts with what you have the most of, goes to what you have the least of
    AmountDecreasing,
    //starts with what you have the least of, goes to what you have the most of
    AmountIncreasing,
}

public class StoredItem
{
    private string name;
    public int amount;

    public string Name
    {
        get
        {
            return name;
        }
    }

    public StoredItem(string name, int amt = 1)
    {
        this.name = name;
        amount = amt;
    }

    /// <summary>
    /// Compares two item, used when sorting the inventory
    /// Sorting order:
    /// Materials (sorted by amount then name if same amount) -> Battle Item (sorted by amount then name if same amount) -> Equippable (sorted by slot then total strength)
    /// </summary>
    /// <param name="other">The item to compare this one to</param>
    /// <returns>1 if the other item is higher up in order, -1 if this item is higher up in order</returns>
    public int CompareTo(StoredItem other)
    {
        if (Registry.ItemRegistry[name] is EquippableBase)
        {
            EquippableBase item1 = ((EquippableBase)Registry.ItemRegistry[name]);
            if (Registry.ItemRegistry[other.name] is EquippableBase)
            {
                EquippableBase item2 = ((EquippableBase)Registry.ItemRegistry[other.name]);
                if (item1.equipSlot > item2.equipSlot)
                {
                    return 1;
                }
                else if (item1.equipSlot < item2.equipSlot)
                {
                    return -1;
                }
                else
                {
                    if (item1.subType > item2.subType)
                    {
                        return 1;
                    }
                    else if (item1.subType < item2.subType)
                    {
                        return -1;
                    }
                    if (item1.health + item1.strength + item1.defense + item1.critChanceMod > item2.health + item2.strength + item2.defense + item2.critChanceMod)
                    {
                        return -1;
                    }
                    else if (item1.health + item1.strength + item1.defense + item1.critChanceMod < item2.health + item2.strength + item2.defense + item2.critChanceMod)
                    {
                        return 1;
                    }
                    else
                    {
                        return name.CompareTo(other.name);
                    }
                }
            }
            else if (Registry.ItemRegistry[other.name] is BattleItemBase)
            {
                return 1;
            }
            else
            {
                return 1;
            }
        }
        else if (Registry.ItemRegistry[name] is BattleItemBase)
        {
            if (Registry.ItemRegistry[other.name] is EquippableBase)
            {
                return -1;
            }
            else if (Registry.ItemRegistry[other.name] is BattleItemBase)
            {
                if (amount > other.amount)
                {
                    return -1;
                }
                else if (amount < other.amount)
                {
                    return 1;
                }
                else
                {
                    //for now just sorting by name, might sort by effect at a later junction
                    return name.CompareTo(other.name);
                }
            }
            else
            {
                return 1;
            }
        }
        else
        {

            if (Registry.ItemRegistry[other.name] is EquippableBase)
            {
                return -1;
            }
            else if (Registry.ItemRegistry[other.name] is BattleItemBase)
            {
                return -1;
            }
            else
            {
                if (amount > other.amount)
                {
                    return -1;
                }
                else if (amount < other.amount)
                {
                    return 1;
                }
                else
                {
                    return name.CompareTo(other.name);
                }
            }
        }
    }
}

public class Inventory
{
    public static List<StoredItem> itemList = new List<StoredItem>();

    public static SortingType sortingType = SortingType.AmountDecreasing;

    public static void LoadInventory()
    {
        if (File.Exists("Assets/Resources/Storage/Inventory.data"))
        {
            Stream inStream = File.OpenRead("Assets/Resources/Storage/Inventory.data");
            BinaryReader file = new BinaryReader(inStream);
            file.Close();
        }
        Debug.Log(AddItem("Animal Tooth", 5));
        Debug.Log(AddItem("Arrows", 19));
        Debug.Log(AddItem("Bandage", 3));
        Debug.Log(AddItem("Bar of Iron", 14));
        Debug.Log(AddItem("Battle Axe", 1));
        Debug.Log(AddItem("Bloodstone Necklace", 1));
        Debug.Log(AddItem("Copper Ore", 24));
        Debug.Log(AddItem("Crossbow", 1));
        Debug.Log(AddItem("Dagger", 1));
        Debug.Log(AddItem("Demonic Sword", 1));
        Debug.Log(AddItem("Egg", 42));
        Debug.Log(AddItem("Empty Bottle (Large)", 2));
        Debug.Log(AddItem("Feather", 42));
        Debug.Log(AddItem("Helmet", 1));
        Debug.Log(AddItem("Magic Dust", 127));
        Debug.Log(AddItem("Mana Potion (Large)", 5));
        Debug.Log(AddItem("Potion of Healing (Large)", 2));
        Debug.Log(AddItem("Ruby", 5));
        AddItem("Wooden Sword", 1);
        AddItem("Iron Sword", 1);
        AddItem("Steel Sword", 1);
        AddItem("Mirendell", 1);

        AddItem("Wooden Lance", 1);
        AddItem("Iron Lance", 1);
        AddItem("Steel Lance", 1);
        AddItem("Sapphire Lance", 1);
        AddItem("Leviantal", 1);

        AddItem("Wooden Axe", 1);
        AddItem("Iron Axe", 1);
        AddItem("Steel Axe", 1);
        AddItem("Xarok", 1);

        AddItem("Staff of Healing", 1);

        SortInventory((int)sortingType);
    }

    public static void SaveInventory()
    {
        Stream outStream = File.OpenWrite("Assets/Resources/Storage/Inventory.data");
        BinaryWriter file = new BinaryWriter(outStream);
        file.Close();
    }

    /// <summary>
    /// Returns a list of items from the inventory filtered in a specified way
    /// </summary>
    /// <param name="filter">Way to filter the returned list.
    ///                      -1 = no filter, 0-6 grabs equippables by the slot they belong in, 7 = all equippables, 8 = all battle items, 9 = all materials</param>
    /// <returns>List of all requested items</returns>
    public static List<StoredItem> GetItemList(int filter)
    {
        List<StoredItem> returnedList = new List<StoredItem>();
        switch (filter)
        {
            case -1:
                foreach (StoredItem i in itemList)
                {
                    returnedList.Add(Copy(i));
                }
                break;
            //If only grabbing equippables for a cerain slot
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
                foreach (StoredItem i in itemList)
                {
                    if (Registry.ItemRegistry[i.Name] is EquippableBase && ((EquippableBase)Registry.ItemRegistry[i.Name]).equipSlot == filter)
                        returnedList.Add(Copy(i));
                }
                break;
            //all equippables
            case 7:
                foreach (StoredItem i in itemList)
                {
                    if (Registry.ItemRegistry[i.Name] is EquippableBase)
                        returnedList.Add(Copy(i));
                }
                break;
            //all battle items
            case 8:
                foreach (StoredItem i in itemList)
                {
                    if (Registry.ItemRegistry[i.Name] is BattleItemBase)
                        returnedList.Add(Copy(i));
                }
                break;
            //all materials
            case 9:
                foreach (StoredItem i in itemList)
                {
                    if (!(Registry.ItemRegistry[i.Name] is EquippableBase || Registry.ItemRegistry[i.Name] is BattleItemBase))
                        returnedList.Add(Copy(i));
                }
                break;
        }
        return returnedList;
    }

    /// <summary>
    /// Adds a new item ot the inventory, making sur not to exceed the max stack for the item if there is one
    /// </summary>
    /// <param name="itemName">Name of the item to add</param>
    /// <param name="amount">Amount to add</param>
    /// <returns>Returns how many of the item were successfully added to the inventory</returns>
    public static int AddItem(string itemName, int amount)
    {
        if (Registry.ItemRegistry[itemName] is EquippableBase)
        {
            itemList.Add(new StoredItem(itemName));
            return 1;
        }
        //if it is either a base material or a battle item, code don't care
        else
        {
            ItemBase item = Registry.ItemRegistry[itemName];
            foreach (StoredItem stored in itemList)
            {
                if (stored.Name == itemName)
                {
                    if (stored.amount + amount >= item.MaxStack)
                    {
                        int amountAccepted = amount - (stored.amount + amount - item.MaxStack);
                        stored.amount = item.MaxStack;
                        return amountAccepted;
                    }
                    else
                    {
                        stored.amount += amount;
                        return amount;
                    }
                }
            }
            //if it doesn't already exist
            itemList.Add(new StoredItem(itemName, Mathf.Min(item.MaxStack, amount)));
            return Mathf.Min(item.MaxStack, amount);
        }
    }

    /// <summary>
    /// Removes an amount of the specified item from the inventory, removing the item itself if there isn't any left
    /// </summary>
    /// <param name="itemName">Name of the item to remove</param>
    /// <param name="amount">Amount to remove</param>
    /// <returns>How many were successfully removed</returns>
    public static int RemoveItem(string itemName, int amount)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].Name == itemName)
            {
                itemList[i].amount -= amount;
                if (itemList[i].amount <= 0)
                {
                    itemList.RemoveAt(i);
                }
                return amount;
            }
        }
        //if it doesn't already exist
        return 0;
    }

    /// <summary>
    /// Gets the amount of an item that exist in the inventory
    /// </summary>
    /// <param name="itemName">Item to count</param>
    /// <returns>Amount of that item that exist in the inventory</returns>
    public static int GetItemAmount(string itemName)
    {
        if (Registry.ItemRegistry[itemName] is EquippableBase) {
            int amt = 0;
            foreach (StoredItem s in itemList)
            {
                if (s.Name == itemName)
                    amt++;
            }
            return amt;
        }
        else
        {
            foreach (StoredItem s in itemList)
            {
                if (s.Name == itemName)
                    return s.amount;
            }
        }
        return 0;
    }

    /// <summary>
    /// Returns a deep copy of the item to avoid pass by reference errors with GetItemList
    /// </summary>
    /// <param name="item">What item to copy</param>
    /// <returns>A deep copy of the given item</returns>
    private static StoredItem Copy(StoredItem item)
    {
        return new StoredItem(item.Name, item.amount);
    }
    
    /// <summary>
    /// Sorts the inventory
    /// </summary>
    /// <param name="sorting">How to sort the inventory</param>
    public static void SortInventory(int sorting)
    {
        itemList.Sort((x, y) => x.CompareTo(y));
        sortingType = (SortingType)sorting;
        if (sortingType == SortingType.AmountIncreasing)
            itemList.Reverse();
    }
}
