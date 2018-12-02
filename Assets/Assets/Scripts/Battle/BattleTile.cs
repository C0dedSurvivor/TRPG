﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTile : MonoBehaviour {

    Material myMat;
    
    public Color skillRangeColor = new Color(0.0f, 0.0f, 0.7f, 0.4f);
    public Color playerMoveInDanger = new Color(1.0f, 0.7f, 0.0f, 0.4f);
    public Color playerMoveArea = new Color(0.0f, 0.7f, 0.0f, 0.4f);
    public Color playerAttackArea = new Color(0.7f, 0.7f, 0.0f, 0.4f);
    public Color dangerArea = new Color(0.7f, 0.0f, 0.0f, 0.4f);
    public Color none = new Color(0.0f, 0.0f, 0.0f, 0.2f);

    public bool playerMoveRange;
    public bool enemyDanger;
    public bool playerAttackRange;
    public bool skillRange;
    public bool skillTargettable;
    public bool skillTargetting;
    //for aoe spells to check if center of spell is legal
    public static bool skillLegitTarget;

    public Vector2Int arrayID;

	// Use this for initialization
	void Start () {
        myMat = GetComponent<Renderer>().material;
        Reset();
	}

    public void UpdateColors()
    {
        if (skillTargetting)
        {
            if (skillLegitTarget)
                myMat.color = playerMoveArea;
            else
                myMat.color = dangerArea;
        }
        else if (skillTargettable)
        {
            myMat.color = playerMoveInDanger;
        }
        else if (skillRange)
        {
            myMat.color = skillRangeColor;
        }
        else if (enemyDanger && playerMoveRange)
        {
            myMat.color = playerMoveInDanger;
        }
        else if (playerMoveRange)
        {
            myMat.color = playerMoveArea;

        }
        else if (playerAttackRange)
        {
            myMat.color = playerAttackArea;
        }
        else if (enemyDanger)
        {
            myMat.color = dangerArea;
        }
        else
        {
            myMat.color = none;
        }
    }

    public void Reset()
    {
        playerMoveRange = false;
        enemyDanger = false;
        playerAttackRange = false;
        skillRange = false;
        skillTargettable = false;
        skillTargetting = false;
        myMat.color = none;
    }

    public void ChangeValueByKey(string tileValue)
    {
        if (tileValue == "danger area")
        {
            enemyDanger = true;
        }
        if (tileValue == "attack area")
        {
            playerAttackRange = true;
        }
    }
}
