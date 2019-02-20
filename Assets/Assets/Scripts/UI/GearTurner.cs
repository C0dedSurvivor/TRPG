using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GearTurner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //How far off the initial button placement is from centered
    public float offset;
    //Amount of buttons that exist
    public int buttonCount;
    //How much you need to rotate from one button to get another
    public float buttonDifference;
    public bool dragging = false;
    //Keeps track of the total rotation of the gear so it doesn't glitch when turned more than 360 degrees
    private float fullRotation = 0;
    //Whether or not the gear can be turned
    public bool frozen = false;
	
	// Update is called once per frame
	void Update () {
        if (!dragging)
        {
            float limiter = (buttonCount / 2.0f) * buttonDifference;
            //Checks to see if the player dragged it beyond where it should be
            if (fullRotation > (limiter + offset) || fullRotation < -(limiter - offset))
            {
                float rotAmt = -0.05f * (fullRotation - Mathf.Deg2Rad * (limiter + offset));
                if (fullRotation > (limiter + offset))
                    rotAmt = -0.05f * (fullRotation - Mathf.Deg2Rad * -(limiter - offset)) / ((limiter + offset) / (limiter - offset));
                transform.Rotate(new Vector3(0, 0, rotAmt));
                fullRotation += rotAmt;
                if (GetComponent<GearInventoryGUI>() != null)
                {
                    GetComponent<GearInventoryGUI>().CheckForOutOfBounds();
                }
                if (GetComponent<PauseInventory>() != null)
                {
                    GetComponent<PauseInventory>().CheckForOutOfBounds();
                }
            }
        }
	}

    /// <summary>
    /// When the player starts dragging the gear
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
    }

    /// <summary>
    /// While the player is dragging the gear, turn the gear and let any attached inventories know
    /// </summary>
    public void OnDrag(PointerEventData data)
    {
        if (!frozen && data.delta.y != 0)
        {
            transform.Rotate(new Vector3(0, 0, data.delta.y / 4.0f));
            fullRotation += data.delta.y / 4.0f;
            if(GetComponent<GearInventoryGUI>() != null)
            {
                GetComponent<GearInventoryGUI>().CheckForOutOfBounds();
            }
            if (GetComponent<PauseInventory>() != null)
            {
                GetComponent<PauseInventory>().CheckForOutOfBounds();
            }
        }
    }

    /// <summary>
    /// When the player stops dragging the gear
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }

    /// <summary>
    /// Resets the rotation
    /// </summary>
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        fullRotation = 0;
    }

    /// <summary>
    /// Rotates so the given button is at the center of the wheel
    /// </summary>
    /// <param name="button">The index of the button to move to</param>
    public void moveToButton(int button)
    {
        transform.rotation = Quaternion.Euler(0, 0, -(buttonCount / 2 - button) * buttonDifference + offset);
        fullRotation = -(buttonCount / 2 - button) * buttonDifference + offset;
    }
}
