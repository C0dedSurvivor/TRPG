/// <summary>
/// A simple pair implementation until Unity allows .Net 4.0
/// </summary>
public class Pair<T1, T2>
{
    public T1 First;
    public T2 Second;

    public Pair(T1 first, T2 second)
    {
        First = first;
        Second = second;
    }
}
