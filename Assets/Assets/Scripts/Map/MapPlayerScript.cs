using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlayerScript : MonoBehaviour {

    public int walkingSpeed = 8;
    public int sprintSpeed = 16;
    public int turningSpeed = 75;

    const double CAMERA_ADJUST_SPEED = 1.5f;
    const int MAX_CAMERA_DISTANCE = 10;
    const int MIN_CAMERA_DISTANCE = 4;

    public GameObject mapCamera;
    public Battle battleController;

	// Use this for initialization
	void Start () {
        //Locks and hides the cursor on startup
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update () {
        if (!PauseGUI.paused && !battleController.IsBattling)
        {
            //Interacts with any objects directly in front of the player that have a PlayerInteraction method
            if (Input.GetKeyDown("r"))
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5.0f))
                {
                    hit.collider.gameObject.BroadcastMessage("PlayerInteraction", gameObject);
                }
            }

            //Movement
            if (Input.GetKey("w") && !Input.GetKey("s"))
            {
                transform.Translate(Vector3.forward * walkingSpeed * Time.deltaTime);
            }
            if (Input.GetKey("s") && !Input.GetKey("w"))
            {
                transform.Translate(Vector3.back * walkingSpeed * Time.deltaTime);
            }
            if (Input.GetKey("a") && !Input.GetKey("d"))
            {
                transform.Translate(Vector3.left * walkingSpeed * Time.deltaTime);
            }
            if (Input.GetKey("d") && !Input.GetKey("a"))
            {
                transform.Translate(Vector3.right * walkingSpeed * Time.deltaTime);
            }
            //Jumping
            if(Input.GetKeyDown(KeyCode.Space) && Physics.Raycast(transform.position + Vector3.down, Vector3.down, 0.001f))
            {
                Debug.Log("jumping");
                GetComponent<Rigidbody>().AddForce(Vector3.up * 300.0f);
            }
            //Camera controls
            if (Input.GetKey(KeyCode.LeftControl))
            {
                float change = Input.GetAxis("Mouse ScrollWheel");
                if ((change > 0 && Vector3.Distance(mapCamera.transform.position, transform.position) > MIN_CAMERA_DISTANCE) || (change < 0 && Vector3.Distance(mapCamera.transform.position, transform.position) < MAX_CAMERA_DISTANCE))
                    mapCamera.transform.Translate(Vector3.forward * change);
            }

            //Turning left and right via the mouse
            transform.Rotate(Input.GetAxis("Mouse X") * transform.up * turningSpeed * Time.deltaTime);
        }
    }
}
