using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Ways to sort the inventory view
/// </summary>
public enum SortingType
{
    //starts with what you have the most of, goes to what you have the least of
    AmountDecreasing,
    //starts with what you have the least of, goes to what you have the most of
    AmountIncreasing,
}

/// <summary>
/// Ways to filter the inventory view
/// </summary>
public enum Filter
{
    All = -1,
    Weapon,
    Helmet,
    Chestplate,
    Legs,
    Boots,
    Gloves,
    Accessory,
    Equippables,
    BattleItems,
    Materials
}

public class Inventory
{
    public static List<StoredItem> itemList = new List<StoredItem>();

    public static SortingType sortingType = SortingType.AmountDecreasing;

    /// <summary>
    /// Loads the inventory from a save file if it exists, otherwise populates the starting inventory
    /// </summary>
    /// <param name="slot">The ID of the save slot</param>
    public static void LoadInventory(int slot)
    {
        if (File.Exists("Assets/Resources/Storage/Slot" + slot + "/Inventory.data"))
        {
            using (StreamReader reader = new StreamReader(StorageDirectory.SaveFilePath + slot + StorageDirectory.InventoryExtension))
            {
                itemList.AddRange(JsonConvert.DeserializeObject<StoredItem[]>(reader.ReadToEnd()));
            }
        }
        else
        {
            Debug.Log(AddItem("Animal Tooth", 5));
            Debug.Log(AddItem("Arrows", 19));
            Debug.Log(AddItem("Bandage", 3));
            Debug.Log(AddItem("Bar of Iron", 14));
            Debug.Log(AddItem(new Equippable("Battle Axe")));
            Debug.Log(AddItem(new Equippable("Bloodstone Necklace")));
            Debug.Log(AddItem("Copper Ore", 24));
            Debug.Log(AddItem(new Equippable("Crossbow")));
            Debug.Log(AddItem(new Equippable("Dagger")));
            Debug.Log(AddItem("Egg", 42));
            Debug.Log(AddItem("Empty Bottle (Large)", 2));
            Debug.Log(AddItem("Feather", 42));
            Debug.Log(AddItem("Magic Dust", 127));
            Debug.Log(AddItem("Mana Potion (Large)", 5));
            Debug.Log(AddItem("Potion of Healing (Large)", 2));
            Debug.Log(AddItem("Ruby", 5));
            AddItem(new Equippable("Wooden Sword"));
            AddItem(new Equippable("Iron Sword"));
            AddItem(new Equippable("Steel Sword"));
            AddItem(new Equippable("Mirendell"));

            AddItem(new Equippable("Wooden Lance"));
            AddItem(new Equippable("Iron Lance"));
            AddItem(new Equippable("Steel Lance"));
            AddItem(new Equippable("Sapphire Lance"));
            AddItem(new Equippable("Leviantal"));

            AddItem(new Equippable("Wooden Axe"));
            AddItem(new Equippable("Iron Axe"));
            AddItem(new Equippable("Steel Axe"));
            AddItem(new Equippable("Xarok"));

            AddItem(new Equippable("Staff of Healing"));
            Debug.Log(AddItem(new Equippable("Helm of Healing")));
            Debug.Log(AddItem(new Equippable("Demonic Sword")));
            Debug.Log(AddItem(new Equippable("Crescent Rose")));
            Debug.Log(AddItem(new Equippable("Chestplate of the Last Stand")));
            Debug.Log(AddItem(new Equippable("Amulet of Rejuvenation")));
            Debug.Log(AddItem(new Equippable("Thorny Leggings")));
            Debug.Log(AddItem(new Equippable("Priest Necklace")));
        }

        SortInventory(sortingType);
    }

    /// <summary>
    /// Save the inventory
    /// </summary>
    /// <param slot="slot">The save slot to save to</param>
    public static void SaveInventory(int slot)
    {
        using (StreamWriter writer = new StreamWriter(StorageDirectory.SaveFilePath + slot + StorageDirectory.InventoryExtension))
        {
            writer.Write(JsonConvert.SerializeObject(itemList));
        }
    }

    /// <summary>
    /// Returns a list of items from the inventory filtered in a specified way
    /// </summary>
    /// <param name="filter">
    /// Way to filter the returned list.
    /// -1 = no filter, 0-6 grabs equippables by the slot they belong in, 7 = all equippables, 8 = all battle items, 9 = all materials
    /// </param>
    /// <returns>List of all requested items</returns>
    public static List<StoredItem> GetItemList(int filter)
    {
        List<StoredItem> returnedList = new List<StoredItem>();
        switch ((Filter)filter)
        {
            case Filter.All:
                foreach (StoredItem i in itemList)
                {
                    returnedList.Add(Copy(i));
                }
                break;
            //If only grabbing equippables for a certain slot
            case Filter.Weapon:
            case Filter.Helmet:
            case Filter.Chestplate:
            case Filter.Legs:
            case Filter.Boots:
            case Filter.Gloves:
            case Filter.Accessory:
                foreach (StoredItem i in itemList)
                {
                    if (Registry.ItemRegistry[i.Name] is EquippableBase && ((EquippableBase)Registry.ItemRegistry[i.Name]).equipSlot == (EquipSlot)filter)
                    {
                        returnedList.Add(Copy(i));
                    }
                }
                break;
            //all equippables
            case Filter.Equippables:
                foreach (StoredItem i in itemList)
                {
                    if (Registry.ItemRegistry[i.Name] is EquippableBase)
                    {
                        returnedList.Add(Copy(i));
                    }
                }
                break;
            //all battle items
            case Filter.BattleItems:
                foreach (StoredItem i in itemList)
                {
                    if (Registry.ItemRegistry[i.Name] is BattleItemBase)
                    {
                        returnedList.Add(Copy(i));
                    }
                }
                break;
            //all materials
            case Filter.Materials:
                foreach (StoredItem i in itemList)
                {
                    if (!(Registry.ItemRegistry[i.Name] is EquippableBase || Registry.ItemRegistry[i.Name] is BattleItemBase))
                    {
                        returnedList.Add(Copy(i));
                    }
                }
                break;
        }
        return returnedList;
    }

    /// <summary>
    /// Adds a material or battle item to the inventory, making sure not to exceed the max stack for the item if there is one
    /// </summary>
    /// <param name="itemName">Name of the item to add</param>
    /// <param name="amount">Amount to add</param>
    /// <returns>Returns how many of the item were successfully added to the inventory</returns>
    public static int AddItem(string itemName, int amount)
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

    public static int AddItem(Equippable equippable)
    {
        itemList.Add(equippable);
        return 1;
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
        if (Registry.ItemRegistry[itemName] is EquippableBase)
        {
            int amt = 0;
            foreach (StoredItem s in itemList)
            {
                if (s.Name == itemName)
                {
                    amt++;
                }
            }
            return amt;
        }
        else
        {
            foreach (StoredItem s in itemList)
            {
                if (s.Name == itemName)
                {
                    return s.amount;
                }
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
        return item is Equippable ? new Equippable((Equippable)item) : new StoredItem(item);
    }

    /// <summary>
    /// Sorts the inventory
    /// </summary>
    /// <param name="sorting">How to sort the inventory</param>
    public static void SortInventory(SortingType sorting)
    {
        if (sorting == SortingType.AmountDecreasing)
        {
            itemList.Sort((x, y) => x.CompareTo(y));
        }
        else
        {
            itemList.Sort((x, y) => y.CompareTo(x));
        }
    }
}
