using UnityEngine;
using System.Collections.Generic;

public class Player : BattleParticipant
{

	private int level;
	private int exp;
	/*
	Stat growth types, determines per level gain: all growth types have a low, normal and high variants
	Flat - same chance per level
	Linear - chance per level increases by the same amount per level
	Reverse Linear - chance per level decreases by the same amount per level
	S Curve - 
	Reverse S Curve - 
	*/
	private string attackGrowthType;
	private string defenseGrowthType;
	private string mAttackGrowthType;
	private string mDefenseGrowthType;
    private string healthGrowthType;

    private int skillPoints = 10;

    public int SkillPoints
    {
        get
        {
            return skillPoints;
        }
    }

    //Skills assigned to the quick cast buttons in battle
    public List<Vector2Int> skillQuickList = new List<Vector2Int>();

    public Player(int x, int y, int mT, string name) : base(name)
    {
        moveType = mT;
        attack = 15 + mT;
        defense = 15 + mT;
        mAttack = 15 + mT;
        mDefense = 15 + mT;
        critChance = 15 + mT;
        mHealth = 30 + mT;
        cHealth = mHealth;
        equippedWeapon = "Wooden Bow";
        position = new Vector2Int(x, y);
        skillQuickList.Add(new Vector2Int(1, 1));
        skillQuickList.Add(new Vector2Int(1, 2));
        skillQuickList.Add(new Vector2Int(1, 3));
    }

	/// <summary>
	/// Equips the new weapon, returning the old one so it can be returned to the inventory. 
	/// </summary>
	public string EquipWeapon(string w){
		string weap = equippedWeapon;
		equippedWeapon = w;
		return weap;
	}

    public void GainExp(int e){
		exp += e;
		CheckForLevel();
	}

    //sees if you have enough EXP to level up and does the level up if so
	private void CheckForLevel(){
		if (exp > GetExpPerLevel ()) {
			exp -= GetExpPerLevel ();
			level++;
            skillPoints++;

			float mod = 1.0f;
			float equation = 0;

			//attack
			if (attackGrowthType.Contains ("low"))
				mod = 0.8f;
			if (attackGrowthType.Contains ("high"))
				mod = 1.2f;

			if (attackGrowthType.Contains ("flat"))
				equation = 35;
			else if (attackGrowthType.Contains ("reverse linear"))
				equation = 65 - level;
			else if (attackGrowthType.Contains ("linear"))
				equation = 25 + level;
			else if (attackGrowthType.Contains ("reverse s curve")) {
				equation = 45 - 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			else if (attackGrowthType.Contains ("s curve")) {
				equation = 45 + 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			for(int i = 0; i < 3; i++)
				if (Random.Range (0.0f, 100.0f) < equation * mod)
					attack++;

			//defense
			if (defenseGrowthType.Contains ("low"))
				mod = 0.8f;
			else if (defenseGrowthType.Contains ("high"))
				mod = 1.2f;
			else
				mod = 1.0f;

			if (defenseGrowthType.Contains ("flat"))
				equation = 35;
			else if (defenseGrowthType.Contains ("reverse linear"))
				equation = 65 - level;
			else if (defenseGrowthType.Contains ("linear"))
				equation = 25 + level;
			else if (defenseGrowthType.Contains ("reverse s curve")) {
				equation = 45 - 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			else if (defenseGrowthType.Contains ("s curve")) {
				equation = 45 + 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			for(int i = 0; i < 3; i++)
				if (Random.Range (0.0f, 100.0f) < equation * mod)
					defense++;

			//magic attack
			if (mAttackGrowthType.Contains ("low"))
				mod = 0.8f;
			else if (mAttackGrowthType.Contains ("high"))
				mod = 1.2f;
			else
				mod = 1.0f;

			if (mAttackGrowthType.Contains ("flat"))
				equation = 35;
			else if (mAttackGrowthType.Contains ("reverse linear"))
				equation = 65 - level;
			else if (mAttackGrowthType.Contains ("linear"))
				equation = 25 + level;
			else if (mAttackGrowthType.Contains ("reverse s curve")) {
				equation = 45 - 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			else if (mAttackGrowthType.Contains ("s curve")) {
				equation = 45 + 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			for(int i = 0; i < 3; i++)
				if (Random.Range (0.0f, 100.0f) < equation * mod)
					mAttack++;

			//magic defense
			if (mDefenseGrowthType.Contains ("low"))
				mod = 0.8f;
			else if (mDefenseGrowthType.Contains ("high"))
				mod = 1.2f;
			else
				mod = 1.0f;

			if (mDefenseGrowthType.Contains ("flat"))
				equation = 35;
			else if (mDefenseGrowthType.Contains ("reverse linear"))
				equation = 65 - level;
			else if (mDefenseGrowthType.Contains ("linear"))
				equation = 25 + level;
			else if (mDefenseGrowthType.Contains ("reverse s curve")) {
				equation = 45 - 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			else if (mDefenseGrowthType.Contains ("s curve")) {
				equation = 45 + 20 * Mathf.Sin ((Mathf.PI * (level - 20)) / 40);
			}
			for(int i = 0; i < 3; i++)
				if (Random.Range (0.0f, 100.0f) < equation * mod)
					mDefense++;

			CheckForLevel();
		}
	}

    //gets how much EXP is needed to get to the next level
	private int GetExpPerLevel(){
		return -20 + level * 50;
    }

    //This pysically cannot be used any more, please fix
    public void UnlockSkill(int treeID, int skillID)
    {
        skillTreeList[treeID][skillID].unlocked = true;
        skillPoints -= skillTreeList[treeID][skillID].unlockCost;
    }
}