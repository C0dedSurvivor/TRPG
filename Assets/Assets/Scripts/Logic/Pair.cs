using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple pair implementation until Unity allows .Net 4.0
/// </summary>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
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
