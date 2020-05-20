public class FunctionEvent : BattleEventBase
{
    public delegate void DefaultType();

    public DefaultType function;

    public FunctionEvent(DefaultType function)
    {
        this.function = function;
    }
}

public class FunctionEvent<T1> : BattleEventBase
{
    public delegate void DefaultType(T1 first);

    public DefaultType function;
    public T1 first;

    public FunctionEvent(DefaultType function, T1 first)
    {
        this.function = function;
        this.first = first;
    }
}

public class FunctionEvent<T1, T2> : BattleEventBase
{
    public delegate void DefaultType(T1 first, T2 second);

    public DefaultType function;
    public T1 first;
    public T2 second;

    public FunctionEvent(DefaultType function, T1 first, T2 second)
    {
        this.function = function;
        this.first = first;
        this.second = second;
    }
}
