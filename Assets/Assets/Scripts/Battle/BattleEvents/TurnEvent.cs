public struct TurnEvent : BattleEventBase
{
    public BattlePawnBase turner;
    public FacingDirection direction;

    public TurnEvent(BattlePawnBase turner, FacingDirection direction)
    {
        this.turner = turner;
        this.direction = direction;
    }
}
