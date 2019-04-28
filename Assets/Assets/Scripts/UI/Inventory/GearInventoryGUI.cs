using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// The base class for Gear-layout inventories
/// </summary>
[RequireComponent(typeof(GearTurner))]
public class GearInventoryGUI : VisualInventoryBase
{
    //The index of the topmost displayed item in the inventory
    private int firstActive = 0;
    //The index of the bottommost displayed item in the inventory
    private int lastActive;
    
    /// <summary>
    /// Generates the initial view of the inventory with a given filter
    /// Filter: -1 = none, 1-6 signify equipment slot, 7 = all equippables, 8 = all battle items, 9 = all materials
    /// </summary>
    public virtual void GenerateInventory()
    {
        firstActive = 0;
        //13 is the amount to display that looks the best, generates less if there are less items to show
        for (int i = 0; i < Mathf.Min(13, itemList.Count); i++)
        {
            itemBoxList.Add(Instantiate(itemBoxPrefab, transform));
            itemBoxList[i].transform.localPosition = new Vector3(GetComponent<Image>().rectTransform.rect.width / 1.9f + 10, 0, 0);
            itemBoxList[i].transform.RotateAround(transform.position, Vector3.forward, -GetComponent<GearTurner>().buttonDifference * (i - ((Mathf.Min(12, itemList.Count) - 1) / 2)));
            itemBoxList[i].GetComponent<InventoryItemButton>().UpdateItem(itemList[i]);
            itemBoxList[i].GetComponent<InventoryItemButton>().item = i;
            int j = i + 1;
            itemBoxList[i].GetComponent<Button>().onClick.AddListener(delegate { MoveToButton(j); });
            lastActive = i;
        }
        GetComponent<GearTurner>().buttonCount = itemList.Count;
        GetComponent<GearTurner>().offset = Mathf.Max(0, ((itemList.Count - 12) / 2.0f) * GetComponent<GearTurner>().buttonDifference);
        GetComponent<GearTurner>().frozen = itemList.Count == 0;
        
        enabled = true;
    }

    /// <summary>
    /// Clears all of the visibles and data
    /// </summary>
    public override void Close()
    {
        base.Close();
        itemList.Clear();
    }

    /// <summary>
    /// Checks to see if any of the item boxes have moved off screen and adjusts them accordingly
    /// </summary>
    public void CheckForOutOfBounds()
    {
        bool neededToMove = false;
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        //If the top item button is not already displaying the top item in the inventory
        if (firstActive != 0)
        {
            Vector3[] v = new Vector3[4];
            itemBoxList[itemBoxList.Count - 1].GetComponent<RectTransform>().GetWorldCorners(v);
            //If the bottom item has moved off the bottom of the screen, move it to the top and display the new item to display
            if (!screenRect.Contains(v[1]) && itemBoxList[itemBoxList.Count - 1].transform.position.y < 0)
            {
                itemBoxList[itemBoxList.Count - 1].transform.SetPositionAndRotation(itemBoxList[0].transform.position, itemBoxList[0].transform.rotation);
                itemBoxList[itemBoxList.Count - 1].transform.RotateAround(transform.position, Vector3.forward, GetComponent<GearTurner>().buttonDifference);
                itemBoxList.Insert(0, itemBoxList[itemBoxList.Count - 1]);
                itemBoxList.RemoveAt(itemBoxList.Count - 1);
                if (firstActive != 0)
                    firstActive--;
                lastActive--;
                Debug.Log(firstActive + "|" + lastActive);
                itemBoxList[0].GetComponent<InventoryItemButton>().UpdateItem(itemList[firstActive]);
                itemBoxList[0].GetComponent<InventoryItemButton>().item = firstActive;
                itemBoxList[0].GetComponent<Button>().onClick.RemoveAllListeners();
                int j = firstActive + 1;
                itemBoxList[0].GetComponent<Button>().onClick.AddListener(delegate { MoveToButton(j); });
                neededToMove = true;
            }
        }
        //If the bottom item button is not already displaying the bottom item in the inventory
        if (lastActive < itemList.Count - 1)
        {
            Vector3[] v = new Vector3[4];
            itemBoxList[0].GetComponent<RectTransform>().GetWorldCorners(v);
            //If the top item has moved off the top of the screen, move it to the bottom and display the new item to display
            if (!screenRect.Contains(v[0]) && itemBoxList[0].transform.position.y > 0)
            {
                itemBoxList[0].transform.SetPositionAndRotation(itemBoxList[itemBoxList.Count - 1].transform.position, itemBoxList[itemBoxList.Count - 1].transform.rotation);
                itemBoxList[0].transform.RotateAround(transform.position, Vector3.forward, -GetComponent<GearTurner>().buttonDifference);
                itemBoxList.Add(itemBoxList[0]);
                itemBoxList.RemoveAt(0);
                firstActive++;
                if (lastActive != itemList.Count - 1)
                    lastActive++;
                Debug.Log(firstActive + "|" + lastActive);
                itemBoxList[itemBoxList.Count - 1].GetComponent<InventoryItemButton>().UpdateItem(itemList[lastActive]);
                itemBoxList[itemBoxList.Count - 1].GetComponent<InventoryItemButton>().item = lastActive;
                itemBoxList[itemBoxList.Count - 1].GetComponent<Button>().onClick.RemoveAllListeners();
                int j = lastActive + 1;
                itemBoxList[itemBoxList.Count - 1].GetComponent<Button>().onClick.AddListener(delegate { MoveToButton(j); });
                neededToMove = true;
            }
        }
        if(neededToMove)
            CheckForOutOfBounds();
    }

    /// <summary>
    /// Centers an item on the wheel when it is clicked
    /// </summary>
    /// <param name="button">What item box to move to</param>
    public virtual void MoveToButton(int button)
    {
        //Moves to the item box
        GetComponent<GearTurner>().moveToButton(button);
        //Checks to see if this pushes any other item boxes off the screen
        CheckForOutOfBounds();
        //Also does what happens when you select an item
        selectedItem = button - 1;
    }

    /// <summary>
    /// Changes the type of inventory to display
    /// </summary>
    /// <param name="filter">How to filter the inventory</param>
    public void ChangeInventory(int filter)
    {
        GetComponent<GearTurner>().ResetRotation();
        Close();
        GenerateInventory();
    }
}
