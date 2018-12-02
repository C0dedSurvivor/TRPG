using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is the thing on the map that starts the battle if you press 'r' while facing it
public class BattleTrigger : MonoBehaviour {

    public Battle battleStuff;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayerInteraction(GameObject player)
    {
        battleStuff.StartBattle(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z), Camera.main.transform);
        player.SetActive(false);
        gameObject.SetActive(false);
    }
}
 