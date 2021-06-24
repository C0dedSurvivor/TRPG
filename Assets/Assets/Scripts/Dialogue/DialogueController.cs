using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    protected List<DialogueNode> toDisplay = new List<DialogueNode>();
    //If dialogue is currently happening, makes sure it doesn't interrupt it with a new line
    private bool animatingText = false;
    //If waiting on a pause to finish
    private bool paused = false;

    [SerializeField]
    private TextAnimator textAnimator;
    [SerializeField]
    private Button branchButtonPrefab;
    [SerializeField]
    private GameObject branchButtonContainer;

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
                //Also handles DialogueLockedChoiceBranch
                case DialogueChoiceBranch choiceBranch:
                    List<string> options = choiceBranch.GetOptions();
                    foreach(string option in options)
                    {
                        Button optionButton = Instantiate(branchButtonPrefab, new Vector3(), Quaternion.Euler(Vector3.zero), branchButtonContainer.transform);
                        string tempString = string.Copy(option);
                        optionButton.GetComponentInChildren<Text>().text = tempString;
                        optionButton.name = tempString;
                        optionButton.onClick.AddListener(delegate { SelectBranch(tempString); });
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
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
                case DialogueAcceptQuest giveQuest:
                    QuestManager.Instance.AcceptQuest(giveQuest.questID);
                    ToNextNode();
                    break;
                case DialogueSubmitQuest submitQuest:
                    QuestManager.Instance.SubmitQuest(submitQuest.questID);
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

    /// <summary>
    /// Registers when a player chooses what branch to go down in a choice branch
    /// </summary>
    /// <param name="choice">The line they chose</param>
    public void SelectBranch(string choice)
    {
        if (toDisplay[0] is DialogueChoiceBranch)
        {
            toDisplay[0] = (toDisplay[0] as DialogueChoiceBranch).GetNext(choice);

            foreach(Transform child in branchButtonContainer.transform)
            {
                Destroy(child.gameObject);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (toDisplay[0] == null)
            {
                toDisplay.RemoveAt(0);
                if (toDisplay.Count == 0)
                    return;
            }
            ProcessNewNode();
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

    /// <summary>
    /// Pauses the dialogue for a given amount of time
    /// </summary>
    /// <param name="time"></param>
    /// <returns>How long to pause for in seconds</returns>
    IEnumerator PauseDialogue(float time)
    {
        paused = true;
        yield return new WaitForSeconds(time);

        paused = false;
        ToNextNode();
    }
}