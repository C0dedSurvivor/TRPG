using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridInventoryGUI : VisualInventoryBase
{
    //The viewport for the inventory display
    public Transform contentArea;

    /// <summary>
    /// Generates the item boxes for all items in the given inventory
    /// </summary>
    public void GenerateInventory()
    {
        Close();
        gameObject.SetActive(true);

        for (int i = 0; i < itemList.Count; i++)
        {
            itemBoxList.Add(Instantiate(itemBoxPrefab, contentArea));
            itemBoxList[i].GetComponent<InventoryItemButton>().UpdateItem(itemList[i]);
            itemBoxList[i].GetComponent<InventoryItemButton>().item = i;
            int j = i;
            itemBoxList[i].GetComponent<Button>().onClick.AddListener(delegate { SelectItem(j); });
        }
        enabled = true;
    }

    /// <summary>
    /// Clears all of the visibles and data
    /// </summary>
    public override void Close()
    {
        base.Close();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Selects an item when the player clicks on it
    /// </summary>
    /// <param name="item">The index of the item they clicked on</param>
    public virtual void SelectItem(int item)
    {
        selectedItem = item;
    }
    
    /// <summary>
    /// Discards an item from the inventory and updates the visuals
    /// Only works for single item stacks
    /// </summary>
    public virtual void Discard()
    {
        itemList.RemoveAt(selectedItem);
        selectedItem = -1;
        GenerateInventory();
    }
}