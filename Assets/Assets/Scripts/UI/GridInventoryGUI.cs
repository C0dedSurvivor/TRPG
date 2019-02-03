using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridInventoryGUI : VisualInventoryBase
{
    public Transform contentArea;

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

    public virtual void Close()
    {
        for (int i = 0; i < itemBoxList.Count; i++)
        {
            Destroy(itemBoxList[i]);
        }
        itemBoxList.Clear();
        gameObject.SetActive(false);
        enabled = false;
    }

    public virtual void SelectItem(int item)
    {
        selectedItem = item;
    }
    
    public virtual void Discard()
    {
        itemList.RemoveAt(selectedItem);
        selectedItem = -1;
        GenerateInventory();
    }
}