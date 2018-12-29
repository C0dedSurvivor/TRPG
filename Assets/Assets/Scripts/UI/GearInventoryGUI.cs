using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(GearTurner))]
public class GearInventoryGUI : MonoBehaviour
{

    public GameObject itemBoxPrefab;

    private List<StoredItem> itemList = new List<StoredItem>();
    private List<GameObject> itemBoxList = new List<GameObject>();
    
    public GameObject itemInfo;
    public Dropdown sorting;
    public Dropdown filter;
    public Button sortAndFilter;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    private int firstActive = 0;
    private int lastActive;

    private Vector3 center;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponentInParent<EventSystem>();
    }

    void Update()
    {
        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);

        bool overItem = false;
        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<InventoryBarButton>() != null)
            {
                overItem = true;
                MouseOverItem(result.gameObject.GetComponent<InventoryBarButton>().item);
            }
        }
        if (!overItem)
            MouseLeaveItem();
    }

    //filter: -1 = none, 1-6 signify equipment slot, 7 = all equippables, 8 = all battle items, 9 = all materials
    public void GenerateInventory()
    {
        center = transform.localPosition;
        Debug.Log(center);
        itemList = Inventory.GetItemList(filter.value - 1);
        firstActive = 0;
        for (int i = 0; i < Mathf.Min(13, itemList.Count); i++)
        {
            itemBoxList.Add(Instantiate(itemBoxPrefab, transform));
            itemBoxList[i].transform.localPosition = new Vector3(GetComponent<Image>().rectTransform.rect.width / 2.0f + 10, 0, 0);
            itemBoxList[i].transform.RotateAround(transform.position, Vector3.forward, -4.5f * (i - ((Mathf.Min(12, itemList.Count) - 1) / 2)));
            itemBoxList[i].GetComponent<InventoryBarButton>().UpdateItem(itemList[i]);
            itemBoxList[i].GetComponent<InventoryBarButton>().item = i;
            int j = i + 1;
            itemBoxList[i].GetComponent<Button>().onClick.AddListener(delegate { MoveToButton(j); });
            lastActive = i;
        }
        GetComponent<GearTurner>().buttonCount = itemList.Count;
        GetComponent<GearTurner>().offset = Mathf.Max(0, ((itemList.Count - 12) / 2.0f) * GetComponent<GearTurner>().buttonDifference);
        GetComponent<GearTurner>().frozen = false;
        
        sorting.gameObject.SetActive(true);
        filter.gameObject.SetActive(true);
        sortAndFilter.gameObject.SetActive(true);

    }

    public void Destroy()
    {
        for (int i = 0; i < itemBoxList.Count; i++)
        {
            Destroy(itemBoxList[i]);
        }
        itemBoxList.Clear();
        itemList.Clear();
        sorting.gameObject.SetActive(false);
        filter.gameObject.SetActive(false);
        sortAndFilter.gameObject.SetActive(false);
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
                itemBoxList[itemBoxList.Count - 1].transform.RotateAround(transform.position, Vector3.forward, 4.5f);
                itemBoxList.Insert(0, itemBoxList[itemBoxList.Count - 1]);
                itemBoxList.RemoveAt(itemBoxList.Count - 1);
                if (firstActive != 0)
                    firstActive--;
                lastActive--;
                Debug.Log(firstActive + "|" + lastActive);
                itemBoxList[0].GetComponent<InventoryBarButton>().UpdateItem(itemList[firstActive]);
                itemBoxList[0].GetComponent<InventoryBarButton>().item = firstActive;
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
                itemBoxList[0].transform.RotateAround(transform.position, Vector3.forward, -4.5f);
                itemBoxList.Add(itemBoxList[0]);
                itemBoxList.RemoveAt(0);
                firstActive++;
                if (lastActive != itemList.Count - 1)
                    lastActive++;
                Debug.Log(firstActive + "|" + lastActive);
                itemBoxList[itemBoxList.Count - 1].GetComponent<InventoryBarButton>().UpdateItem(itemList[lastActive]);
                itemBoxList[itemBoxList.Count - 1].GetComponent<InventoryBarButton>().item = lastActive;
                itemBoxList[itemBoxList.Count - 1].GetComponent<Button>().onClick.RemoveAllListeners();
                int j = lastActive + 1;
                itemBoxList[itemBoxList.Count - 1].GetComponent<Button>().onClick.AddListener(delegate { MoveToButton(j); });
                neededToMove = true;
            }
        }
        if(neededToMove)
            CheckForOutOfBounds();
    }

    public void MoveToButton(int button)
    {
        GetComponent<GearTurner>().moveToButton(button);
        CheckForOutOfBounds();
    }

    public void SortAndChangeFilter()
    {
        Inventory.SortInventory(sorting.value);
        ChangeInventory(filter.value - 1);
    }

    public void ChangeInventory(int filter)
    {
        GetComponent<GearTurner>().ResetRotation();
        Destroy();
        GenerateInventory();
    }

    public void MouseOverItem(int item)
    {
        itemInfo.SetActive(true);
        itemInfo.transform.position = Input.mousePosition + new Vector3(2, -2, 0);
        //if (itemInfo.transform.localPosition.y < 0 && Mathf.Abs(itemInfo.transform.localPosition.y) + itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight > Screen.height / 2)
        //    itemInfo.transform.position = new Vector3(itemInfo.transform.position.x, Screen.height / 2 - itemInfo.GetComponent<VerticalLayoutGroup>().preferredHeight / 2, itemInfo.transform.position.z);
        itemInfo.transform.GetChild(0).GetComponent<Text>().text = itemList[item].Name;
        if(Registry.ItemRegistry[itemList[item].Name] is EquippableBase)
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
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Health: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).health + "\n";
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength != 0)
            {
                if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).subType == 0)
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Physical";
                else
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "AEtheric";
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += " Strength: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).strength + "\n";
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense != 0)
            {
                if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).subType == 0)
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Physical";
                else
                    itemInfo.transform.GetChild(2).GetComponent<Text>().text += "AEtheric";
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += " Defense: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).defense + "\n";
            }
            if (((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod != 0)
                itemInfo.transform.GetChild(2).GetComponent<Text>().text += "Crit Chance: " + ((EquippableBase)Registry.ItemRegistry[itemList[item].Name]).critChanceMod + "%";
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

    public void MouseLeaveItem()
    {
        itemInfo.SetActive(false);
    }
}
