using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doorway : MonoBehaviour {

    public Vector3 position;
    public Vector3 rotation;

    public void PlayerInteraction(GameObject player)
    {
        player.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
    }
}
