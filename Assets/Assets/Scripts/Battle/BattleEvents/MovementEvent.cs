using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MovementEvent : BattleEventBase
{
    public GameObject mover;
    public float speed;
    public Vector3 initialPosition;
    public Vector3 finalPosition;
    public Quaternion initialRotation;
    public Quaternion finalRotation;
    //If true animation will have no acceleration, if false object will slow down as it reaches its destination
    public bool flatSpeed;

    public MovementEvent(GameObject target, float speed, Vector3 initialPos, Vector3 finalPos, Quaternion initialRot, Quaternion finalRot, bool flatSpeed)
    {
        mover = target;
        this.speed = speed;
        initialPosition = initialPos;
        finalPosition = finalPos;
        initialRotation = initialRot;
        finalRotation = finalRot;
        this.flatSpeed = flatSpeed;
    }
}
