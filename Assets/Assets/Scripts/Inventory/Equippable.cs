using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Currently is just a way to store equippable items as a stored item
/// Allows for equippables to have unique functionality as individual instances compared to the generic version
/// </summary>
public class Equippable : StoredItem
{
    public Equippable(string name) : base(name, 1) { }

    public Equippable(Equippable other) : base(other.name, 1) { }
}
