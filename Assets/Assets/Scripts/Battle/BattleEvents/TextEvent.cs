public class TextEvent : BattleEventBase
{
    public string text;

    public TextEvent(string textToDisplay)
    {
        text = textToDisplay;
    }
}
