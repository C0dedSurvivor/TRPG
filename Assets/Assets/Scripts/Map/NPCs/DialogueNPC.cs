using UnityEngine;

public class DialogueNPC : MonoBehaviour, IMapInteractable
{
    [SerializeField]
    private string dialogueName;

    [SerializeField]
    private DialogueController dialogueController;

    /// <summary>
    /// Shows the dialogue controller and starts the dialogue
    /// </summary>
    /// <param name="player"></param>
    public void PlayerInteraction(GameObject player)
    {
        Debug.Log("Starting dialogue with " + name + ".");
        dialogueController.StartDialogue(dialogueName);
    }
}
