using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlayerScript : MonoBehaviour {

    public int walkingSpeed = 8;
    public int sprintSpeed = 16;
    public int turningSpeed = 75;

	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void Update () {
        if (!PauseGUI.paused && Battle.matchPart == "")
        {
            //only for starting battles right now, later can be used for doors and NPCs
            if (Input.GetKeyDown("r"))
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5.0f))
                {
                    hit.collider.gameObject.BroadcastMessage("PlayerInteraction", gameObject);
                }
            }

            //walk
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
            if(Input.GetKeyDown(KeyCode.Space) && Physics.Raycast(transform.position + Vector3.down, Vector3.down, 0.001f))
            {
                Debug.Log("jumping");
                GetComponent<Rigidbody>().AddForce(Vector3.up * 300.0f);
            }

            //turn
            transform.Rotate(Input.GetAxis("Mouse X") * transform.up * turningSpeed * Time.deltaTime);
        }
    }
}
