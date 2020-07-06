using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    protected List<DialogueNode> toDisplay = new List<DialogueNode>();
    [SerializeField]
    private TextAnimator textAnimator;
    //If dialogue is currently happening, makes sure it doesn't interrupt it with a new line
    private bool animatingText = false;
    //If waiting on a pause to finish
    private bool paused = false;

    /// <summary>
    /// Is true if the text animator is done displaying all text from the queue
    /// </summary>
    public bool Done { get { return textAnimator.Done && toDisplay.Count == 0; } }

    void Start()
    {
        if (textAnimator == null)
            textAnimator = GetComponent<TextAnimator>();
    }

    /// <summary>
    /// Steps the animation every time the timer reaches a certain delay and checks for input to go to the next line
    /// </summary>
    protected void Update()
    {
        if (animatingText && textAnimator.Done)
        {
            animatingText = false;
            ToNextNode();
        }
    }

    /// <summary>
    /// Adds and starts the dialogue tree from an interactable if this is not already in use
    /// </summary>
    /// <param name="dialogueName">The name of the dialogue tree to display</param>
    public void StartDialogue(string dialogueName)
    {
        if (Done)
        {
            Enqueue(Registry.DialogueRegistry[dialogueName]);
        }
    }

    /// <summary>
    /// Finds out what the next node to act on is, returns if there's nothing else to do
    /// </summary>
    private void ToNextNode()
    {
        toDisplay[0] = toDisplay[0]?.GetNext();
        if (toDisplay[0] == null)
        {
            toDisplay.RemoveAt(0);
            if (toDisplay.Count == 0)
                return;
        }
        ProcessNewNode();
    }

    /// <summary>
    /// Sets up the currently queued node to be acted on
    /// </summary>
    private void ProcessNewNode()
    {
        if (toDisplay.Count > 0 && !paused)
        {
            DialogueNode next = toDisplay[0];
            switch (next)
            {
                case DialogueLine line:
                    textAnimator.Enqueue(line);
                    animatingText = true;
                    break;
                case DialogueConditionalBranch condBranch:
                    ToNextNode();
                    break;
                case DialogueChoiceBranch choiceBranch:
                    ToNextNode();
                    break;
                case DialogueLockedChoiceBranch lockedChoiceBranch:
                    ToNextNode();
                    break;
                case DialoguePause pause:
                    StartCoroutine(PauseDialogue(pause.seconds));
                    break;
                case DialogueVisibleOther objVis:
                    objVis.target.SetActive(objVis.newVisibility);
                    ToNextNode();
                    break;
                case DialogueVisibleSelf objVis:
                    gameObject.SetActive(objVis.newVisibility);
                    ToNextNode();
                    break;
                case DialogueSpriteSwitch spriteSwitch:
                    //spriteSwitch.target.sprite = GameStorage.sprites[spriteSwitch.newTexture];
                    ToNextNode();
                    break;
                case DialogueGiveQuest giveQuest:
                    ToNextNode();
                    break;
                case DialogueQuestReward questReward:
                    ToNextNode();
                    break;
                case DialogueTriggerBattle battleTrigger:
                    ToNextNode();
                    break;
                case DialogueCanPlayerMove playerMove:
                    GameStorage.mapPlayer.CanMove = playerMove.canMove;
                    ToNextNode();
                    break;
            }
        }
    }

    public void SelectChoice(int choice)
    {
        if(toDisplay[0] is DialogueChoiceBranch)
        {
            (toDisplay[0] as DialogueChoiceBranch).SetSelected(choice);
            ToNextNode();
        }
    }

    /// <summary>
    /// Adds a dialog set to the queue of text to show
    /// </summary>
    /// <param name="line">The dialog set to display</param>
    public void Enqueue(DialogueNode line)
    {
        toDisplay.Add(line);
        if (toDisplay.Count == 1)
            ProcessNewNode();
    }

    IEnumerator PauseDialogue(float time)
    {
        paused = true;
        yield return new WaitForSeconds(time);

        paused = false;
        ToNextNode();
    }
}