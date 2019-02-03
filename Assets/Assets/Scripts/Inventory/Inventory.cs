﻿using System.Collections;
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

    public int CompareTo(StoredItem m)
    {
        if (Registry.ItemRegistry[name] is EquippableBase)
        {
            EquippableBase item1 = ((EquippableBase)Registry.ItemRegistry[name]);
            if (Registry.ItemRegistry[m.name] is EquippableBase)
            {
                EquippableBase item2 = ((EquippableBase)Registry.ItemRegistry[m.name]);
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
                        return name.CompareTo(m.name);
                    }
                }
            }
            else if (Registry.ItemRegistry[m.name] is BattleItemBase)
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
            if (Registry.ItemRegistry[m.name] is EquippableBase)
            {
                return -1;
            }
            else if (Registry.ItemRegistry[m.name] is BattleItemBase)
            {
                if (amount > m.amount)
                {
                    return -1;
                }
                else if (amount < m.amount)
                {
                    return 1;
                }
                else
                {
                    //for now just sorting by name, might sort by effect at a later junction
                    return name.CompareTo(m.name);
                }
            }
            else
            {
                return 1;
            }
        }
        else
        {

            if (Registry.ItemRegistry[m.name] is EquippableBase)
            {
                return -1;
            }
            else if (Registry.ItemRegistry[m.name] is BattleItemBase)
            {
                return -1;
            }
            else
            {
                if (amount > m.amount)
                {
                    return -1;
                }
                else if (amount < m.amount)
                {
                    return 1;
                }
                else
                {
                    return name.CompareTo(m.name);
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

    public static int GetItemAmount(string i)
    {
        if (Registry.ItemRegistry[i] is EquippableBase) {
            int amt = 0;
            foreach (StoredItem s in itemList)
            {
                if (s.Name == i)
                    amt++;
            }
            return amt;
        }
        else
        {
            foreach (StoredItem s in itemList)
            {
                if (s.Name == i)
                    return s.amount;
            }
        }
        return 0;
    }

    private static StoredItem Copy(StoredItem i)
    {
        return new StoredItem(i.Name, i.amount);
    }

    public static void SortInventory(int sorting)
    {
        itemList.Sort((x, y) => x.CompareTo(y));
        sortingType = (SortingType)sorting;
        if (sortingType == SortingType.AmountIncreasing)
            itemList.Reverse();
    }
}