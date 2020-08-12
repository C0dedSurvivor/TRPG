using UnityEngine;
using UnityEngine.UI;

public class TextAnimator : MonoBehaviour
{
    private const float delay = 0.05f;

    private string currentLine = null;
    private float timer = 0;
    [SerializeField]
    private Text speaker;
    [SerializeField]
    private Text text;

    /// <summary>
    /// Is true if the text animator is done displaying all text from the queue
    /// </summary>
    public bool Done { get { return currentLine == null; } }

    void Start()
    {
        if (text == null)
            text = GetComponent<Text>();
    }

    /// <summary>
    /// Steps the animation every time the timer reaches a certain delay and checks for input to go to the next line
    /// </summary>
    protected void Update()
    {
        if (currentLine != null)
        {
            if (timer >= delay)
            {
                if (currentLine != text.text)
                    StepCurrent();
            }
            else
                timer += Time.deltaTime;
            if (InputManager.KeybindTriggered(PlayerKeybinds.UIContinueText) && currentLine == text.text)
            {
                speaker.text = "";
                text.text = "";
                currentLine = null;
            }
            else if (InputManager.KeybindTriggered(PlayerKeybinds.UISkipText))
            {
                text.text = currentLine;
            }
        }
    }

    /// <summary>
    /// Adds a text event to the queue of text to show
    /// </summary>
    /// <param name="message">The text event to display</param>
    public void Enqueue(TextEvent message)
    {
        currentLine = message.text;
    }

    /// <summary>
    /// Adds a dialog set to the queue of text to show
    /// </summary>
    /// <param name="line">The dialog set to display</param>
    public void Enqueue(DialogueLine line)
    {
        if (line.speaker != null)
            speaker.text = line.speaker;
        text.text = "";
        currentLine = line.line;
    }

    /// <summary>
    /// Steps the current text animaton by one letter
    /// </summary>
    private void StepCurrent()
    {
        text.text = currentLine.Substring(0, text.text.Length + 1);
        timer = 0;
    }
}
