using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VisualInventoryBase : MonoBehaviour {

    public GameObject itemBoxPrefab;

    protected List<StoredItem> itemList = new List<StoredItem>();
    protected List<GameObject> itemBoxList = new List<GameObject>();

    protected GraphicRaycaster m_Raycaster;
    protected PointerEventData m_PointerEventData;
    protected EventSystem m_EventSystem;

    public GameObject itemInfo;
    protected int selectedItem;

    protected bool enabled = false;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponentInParent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponentInParent<EventSystem>();
    }

    void Update()
    {
        if (enabled)
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
            //Shows the item info if the player is mousing over an item
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<InventoryItemButton>() != null)
                {
                    overItem = true;
                    MouseOverItem(result.gameObject.GetComponent<InventoryItemButton>().item);
                }
            }
            if (!overItem)
                MouseLeaveItem();
        }
    }

    //
    /// <summary>
    /// Makes sure whatever gets shown stays within the bounds of the screen
    /// To be overridden with that info needs to be shown then called
    /// </summary>
    public virtual void MouseOverItem(int item)
    {
        RectTransform infoTransform = itemInfo.GetComponent<RectTransform>();
        if (infoTransform.position.x + infoTransform.sizeDelta.x * infoTransform.lossyScale.x > Screen.width)
            itemInfo.transform.position = new Vector3(Screen.width - infoTransform.sizeDelta.x * infoTransform.lossyScale.x, itemInfo.transform.position.y, itemInfo.transform.position.z);
        if (infoTransform.position.y - infoTransform.sizeDelta.y * infoTransform.lossyScale.y < 0)
            itemInfo.transform.position = new Vector3(itemInfo.transform.position.x, infoTransform.sizeDelta.y * infoTransform.lossyScale.y, itemInfo.transform.position.z);
    }

    /// <summary>
    /// Hides the skill info when the player is no longer mousing over an item
    /// </summary>
    public virtual void MouseLeaveItem()
    {
        itemInfo.SetActive(false);
    }
}
