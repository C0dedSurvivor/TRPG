using System;

public class StoredItem : IComparable<StoredItem>
{
    protected string name;
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

    public StoredItem(StoredItem other)
    {
        name = other.name;
        amount = other.amount;
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
                    if (item1.TotalStats > item2.TotalStats)
                    {
                        return -1;
                    }
                    else if (item1.TotalStats < item2.TotalStats)
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