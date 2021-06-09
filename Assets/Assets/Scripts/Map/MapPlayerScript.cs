using System.Collections.Generic;
using UnityEngine;

public class MapPlayerScript : MonoBehaviour
{
    public int walkingSpeed = 8;
    public int sprintSpeed = 16;
    public int turningSpeed = 75;
    public int jumpStrength = 300;

    const double CAMERA_ADJUST_SPEED = 1.5f;
    const int MAX_CAMERA_DISTANCE = 10;
    const int MIN_CAMERA_DISTANCE = 4;

    private Rigidbody rigidbody;

    public Camera mapCamera;
    public Battle battleController;

    private bool canMove = true;

    public bool CanMove { set { canMove = value; } }

    // Use this for initialization
    void Start()
    {
        //Locks and hides the cursor on startup
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rigidbody = GetComponent<Rigidbody>();
        CanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseGUI.paused && canMove)
        {
            //Interacts with any objects directly in front of the player that have a PlayerInteraction method
            if (InputManager.KeybindTriggered(PlayerKeybinds.MapInteract))
            {
                RaycastHit hit;
                IMapInteractable interactable;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5.0f) &&
                    (interactable = hit.collider.gameObject.GetComponent<IMapInteractable>()) != null)
                {
                    interactable.PlayerInteraction(gameObject);
                }
            }

            //Movement
            Vector3 movement = Vector3.zero;
            movement += InputManager.KeybindTriggered(PlayerKeybinds.MapMoveForward) ? Vector3.forward : Vector3.zero;
            movement += InputManager.KeybindTriggered(PlayerKeybinds.MapMoveBack) ? Vector3.back : Vector3.zero;
            movement += InputManager.KeybindTriggered(PlayerKeybinds.MapMoveLeft) ? Vector3.left : Vector3.zero;
            movement += InputManager.KeybindTriggered(PlayerKeybinds.MapMoveRight) ? Vector3.right : Vector3.zero;
            transform.Translate(movement.normalized * walkingSpeed * Time.deltaTime);
            
            // ***
            //Test quests
            // ***
            QuestManager.Instance.CheckProgression(new QuestPacket(LoggableAction.Travel, new List<QuestReqActionMod>() { new QuestReqActionMod(QuestActionModType.Where, "IDK") }, movement.magnitude));
            QuestManager.Instance.CheckProgression(new QuestPacket(LoggableAction.Travel, new List<QuestReqActionMod>() { }, movement.magnitude));

            //Jumping
            if (InputManager.KeybindTriggered(PlayerKeybinds.MapJump) && Physics.Raycast(transform.position + Vector3.down, Vector3.down, 0.001f))
            {
                Debug.Log("jumping");
                rigidbody.AddForce(Vector3.up * jumpStrength);
            }

            //Camera controls
            if (InputManager.KeybindTriggered(PlayerKeybinds.MapAdjustCameraDistance))
            {
                //Make sure the camera stays within bounds
                float change = Input.GetAxis("Mouse ScrollWheel");
                if ((change > 0 && Vector3.Distance(mapCamera.transform.position, transform.position) > MIN_CAMERA_DISTANCE) ||
                    (change < 0 && Vector3.Distance(mapCamera.transform.position, transform.position) < MAX_CAMERA_DISTANCE))
                    mapCamera.transform.Translate(Vector3.forward * change);
            }

            //Turning left and right via the mouse
            transform.Rotate(Input.GetAxis("Mouse X") * transform.up * turningSpeed * Time.deltaTime);
        }
    }
}
