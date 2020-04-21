using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextAnimator : MonoBehaviour
{
    private const float delay = 0.05f;

    private Queue<TextEvent> toAnimate = new Queue<TextEvent>();
    private TextEvent currentAnim = null;
    private float timer = 0;
    private Text text;

    /// <summary>
    /// Is true if the text animator is done displaying all text from the queue
    /// </summary>
    public bool Done { get { return currentAnim == null && toAnimate.Count == 0; } }

    void Start()
    {
        text = GetComponent<Text>();
    }

    /// <summary>
    /// Steps the animation every time the timer reaches a certain delay and checks for input to go to the next line
    /// </summary>
    void Update()
    {
        if (currentAnim != null)
        {
            if (timer >= delay)
            {
                if (currentAnim.text != text.text)
                    StepCurrent();
            }
            else
                timer += Time.deltaTime;
            if (InputManager.KeybindTriggered(PlayerKeybinds.UIContinueText) && currentAnim.text == text.text)
            {
                currentAnim = null;
                text.text = "";
            }
            else if (InputManager.KeybindTriggered(PlayerKeybinds.UISkipText))
            {
                text.text = currentAnim.text;
            }
        }
        else if (toAnimate.Count > 0)
        {
            text.text = "";
            currentAnim = toAnimate.Dequeue();
        }
    }

    /// <summary>
    /// Adds a text event to the queue of text to show
    /// </summary>
    /// <param name="animation">The string to display</param>
    public void Enqueue(TextEvent animation)
    {
        toAnimate.Enqueue(animation);
    }

    /// <summary>
    /// Steps the current text animaton by one letter
    /// </summary>
    private void StepCurrent()
    {
        text.text = currentAnim.text.Substring(0, text.text.Length + 1);
        timer = 0;
    }
}
