using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When attached to a gameobject, starts a battle if the player presses 'r' while facing it
///Is a temporary test implementation
/// </summary>
public class BattleTrigger : MonoBehaviour {

    public Battle battleStuff;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Triggered when the player presses 'r' while facing this object
    /// </summary>
    /// <param name="player">Object that triggered this interaction</param>
    public void PlayerInteraction(GameObject player)
    {
        //starts the battle at this object's position
        battleStuff.StartBattle(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), Camera.main.transform);
        //Stops the player from moving, being rendered or interacted with
        player.SetActive(false);
        //Stops this gameObject from being rendered or interacted with
        gameObject.SetActive(false);
    }
}
 