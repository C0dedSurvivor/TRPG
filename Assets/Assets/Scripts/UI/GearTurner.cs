using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GearTurner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public float offset;
    public int buttonCount;
    public float buttonDifference;
    public bool dragging = false;
    private float fullRotation = 0;
    public bool frozen = false;
	
	// Update is called once per frame
	void Update () {
        if (!dragging)
        {
            float limiter = (buttonCount / 2.0f) * buttonDifference;
            if (fullRotation > (limiter + offset) || fullRotation < -(limiter - offset))
            {
                transform.Rotate(new Vector3(0, 0, -0.05f * ((fullRotation) - Mathf.Deg2Rad * (limiter + offset))));
                fullRotation += -0.05f * ((fullRotation) - Mathf.Deg2Rad * (limiter + offset));
                if (GetComponent<GearInventoryGUI>() != null)
                {
                    GetComponent<GearInventoryGUI>().CheckForOutOfBounds();
                }
            }
        }
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
    }

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
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        fullRotation = 0;
    }

    public void moveToButton(int button)
    {
        transform.rotation = Quaternion.Euler(0, 0, -(buttonCount / 2 - button) * buttonDifference + offset);
        fullRotation = -(buttonCount / 2 - button) * buttonDifference + offset;
    }
}
