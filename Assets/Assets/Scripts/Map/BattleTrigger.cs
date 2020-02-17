using UnityEngine;

/// <summary>
/// When attached to a gameobject, starts a battle if the player presses 'r' while facing it
///Is a temporary test implementation
/// </summary>
public class BattleTrigger : MonoBehaviour, IMapInteractable
{

    public Battle battleController;

    /// <summary>
    /// Triggered when the player presses 'r' while facing this object
    /// </summary>
    /// <param name="player">Object that triggered this interaction</param>
    public void PlayerInteraction(GameObject player)
    {
        //starts the battle at this object's position
        battleController.StartBattle(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), 
            player.GetComponent<MapPlayerScript>().mapCamera.transform);
        //Stops the player from moving, being rendered or interacted with
        player.SetActive(false);
        //Stops this gameObject from being rendered or interacted with
        gameObject.SetActive(false);
    }
}
