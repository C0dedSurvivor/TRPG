using UnityEngine;

interface IMapInteractable
{
    /// <summary>
    /// Triggered when the player presses the interact key while facing this object
    /// </summary>
    /// <param name="player">Object that triggered this interaction</param>
    void PlayerInteraction(GameObject player);
}