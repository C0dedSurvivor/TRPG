﻿using UnityEngine;

/// <summary>
/// What directions a move event can force a pawn to move in
/// </summary>
public enum MoveDirection
{
    Up,
    Right,
    Down,
    Left,
    Random,
    //If knockback from an effect
    FromCenter
}

public class MovePart : SkillPartBase
{
    public MoveDirection direction;
    public int amount;
    //False if this movement was made by the pawn/from the same team
    public bool forced;
    //If this is a movement that depends on where the center of the effect is coming from (explosive movement)
    public Vector2Int center;

    public MovePart() { }

    public MovePart(TargettingType target, MoveDirection direction, int distance, Vector2Int center, bool forced = false, int chance = 100) : base(target, chance)
    {
        this.direction = direction;
        amount = distance;
        this.forced = forced;
        this.center = center;
    }
}
