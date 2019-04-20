using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextAnimator : MonoBehaviour
{
    private const float delay = 0.1f;

    private Queue<TextEvent> toAnimate = new Queue<TextEvent>();
    private TextEvent currentAnim = null;
    private float timer = 0;
    private Text text;

    public bool Done { get { return currentAnim == null && toAnimate.Count == 0; } }

    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentAnim != null)
        {
            if (timer >= delay)
            {
                if (currentAnim.text != text.text)
                    StepCurrent();
            }
            else
                timer += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (currentAnim.text == text.text)
                {
                    currentAnim = null;
                    text.text = "";
                }
                else
                    text.text = currentAnim.text;
            }
        }
        else if(toAnimate.Count > 0)
        {
            text.text = "";
            currentAnim = toAnimate.Dequeue();
        }
    }

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
