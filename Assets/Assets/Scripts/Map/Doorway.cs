using UnityEngine;

/// <summary>
/// When attached to a gameobject, teleports the player if they press 'r' while facing it
/// </summary>
public class Doorway : MonoBehaviour, IMapInteractable
{
    //Where to teleport the player to on interaction
    public Vector3 exitPosition;
    //What the direction the player should face post-movement
    public Vector3 exitRotation;

    /// <summary>
    /// Triggered when the player presses 'r' while facing this object
    /// </summary>
    /// <param name="player">Object that triggered this interaction</param>
    public void PlayerInteraction(GameObject player)
    {
        //Teleports the player
        player.transform.SetPositionAndRotation(exitPosition, Quaternion.Euler(exitRotation));
    }
}
