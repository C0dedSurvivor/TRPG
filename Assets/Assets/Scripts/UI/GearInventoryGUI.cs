using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(GearTurner))]
public class GearInventoryGUI : VisualInventoryBase
{
    private int firstActive = 0;
    private int lastActive;

    //filter: -1 = none, 1-6 signify equipment slot, 7 = all equippables, 8 = all battle items, 9 = all materials
    public virtual void GenerateInventory()
    {
        firstActive = 0;
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
        GetComponent<GearTurner>().frozen = false;
        
        enabled = true;
    }

    public virtual void Destroy()
    {
        for (int i = 0; i < itemBoxList.Count; i++)
        {
            Destroy(itemBoxList[i]);
        }
        itemBoxList.Clear();
        itemList.Clear();
        enabled = false;
    }

    public void CheckForOutOfBounds()
    {
        bool neededToMove = false;
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        if (firstActive != 0)
        {
            Vector3[] v = new Vector3[4];
            itemBoxList[itemBoxList.Count - 1].GetComponent<RectTransform>().GetWorldCorners(v);
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
        if (lastActive < itemList.Count - 1)
        {
            Vector3[] v = new Vector3[4];
            itemBoxList[0].GetComponent<RectTransform>().GetWorldCorners(v);
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

    public virtual void MoveToButton(int button)
    {
        GetComponent<GearTurner>().moveToButton(button);
        CheckForOutOfBounds();
        //also does what happens when you select an item
        selectedItem = button - 1;
    }

    public void ChangeInventory(int filter)
    {
        GetComponent<GearTurner>().ResetRotation();
        Destroy();
        GenerateInventory();
    }

    public virtual void Discard(){}
}
