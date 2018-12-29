using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
	private string attackGrowthType = "TempString";
	private string defenseGrowthType = "TempString";
	private string mAttackGrowthType = "TempString";
	private string mDefenseGrowthType = "TempString";
    private string healthGrowthType = "TempString";

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
        position = new Vector2Int(x, y);
        if (!File.Exists("Assets/Resources/Storage/Players/" + name + ".data")) {
            moveType = mT;
            attack = 15 + mT;
            defense = 15 + mT;
            mAttack = 15 + mT;
            mDefense = 15 + mT;
            critChance = 15 + mT;
            mHealth = 150 + mT;
            cHealth = mHealth / 2;
            equippedWeapon = "Wooden Bow";
            skillQuickList.Add(new Vector2Int(1, 1));
            skillQuickList.Add(new Vector2Int(1, 2));
            skillQuickList.Add(new Vector2Int(1, 3));

            //grab all the skills
            List<int> treeList = GameStorage.GetPlayerSkillList(name);
            foreach (int tree in treeList)
            {
                skillTreeList.Add(tree, new Dictionary<int, SkillInfo>());
                foreach (int skill in GameStorage.skillTreeList[tree].Keys) {
                    skillTreeList[tree].Add(skill, new SkillInfo());
                    if (GameStorage.skillTreeList[tree][skill].dependencies.Count == 0)
                        skillTreeList[tree][skill].unlocked = true;
                }
            }
            SavePlayer();
        }
        else
        {
            LoadPlayer();
        }
    }

    public void SavePlayer()
    {
        Debug.Log("Saving the player");
        Stream outStream = File.OpenWrite("Assets/Resources/Storage/Players/" + name + ".data");
        BinaryWriter file = new BinaryWriter(outStream);
        file.Write(level);
        file.Write(exp);
        file.Write(skillPoints);
        file.Write(moveType);
        file.Write(attack);
        file.Write(attackGrowthType);
        file.Write(defense);
        file.Write(defenseGrowthType);
        file.Write(mAttack);
        file.Write(mAttackGrowthType);
        file.Write(mDefense);
        file.Write(mDefenseGrowthType);
        file.Write(critChance);
        file.Write(cHealth);
        file.Write(mHealth);
        file.Write(healthGrowthType);
        file.Write(equippedWeapon);
        file.Write(skillQuickList[0].x);
        file.Write(skillQuickList[0].y);
        file.Write(skillQuickList[1].x);
        file.Write(skillQuickList[1].y);
        file.Write(skillQuickList[2].x);
        file.Write(skillQuickList[2].y);
        file.Write(skillTreeList.Count);
        foreach(int tree in skillTreeList.Keys)
        {
            file.Write(tree);
            file.Write(skillTreeList[tree].Count);
            foreach (int skill in skillTreeList[tree].Keys)
            {
                file.Write(skill);
                file.Write(skillTreeList[tree][skill].unlocked);
                file.Write(skillTreeList[tree][skill].level);
            }
        }
        file.Close();
    }

    public void LoadPlayer()
    {
        Stream inStream = File.OpenRead("Assets/Resources/Storage/Players/" + name + ".data");
        BinaryReader file = new BinaryReader(inStream);
        level = file.ReadInt32();
        exp = file.ReadInt32();
        skillPoints = file.ReadInt32();
        moveType = file.ReadInt32();
        attack = file.ReadInt32();
        attackGrowthType = file.ReadString();
        defense = file.ReadInt32();
        defenseGrowthType = file.ReadString();
        mAttack = file.ReadInt32();
        mAttackGrowthType = file.ReadString();
        mDefense = file.ReadInt32();
        mDefenseGrowthType = file.ReadString();
        critChance = file.ReadInt32();
        cHealth = file.ReadInt32();
        mHealth = file.ReadInt32();
        healthGrowthType = file.ReadString();
        equippedWeapon = file.ReadString();
        skillQuickList.Add(new Vector2Int(file.ReadInt32(), file.ReadInt32()));
        skillQuickList.Add(new Vector2Int(file.ReadInt32(), file.ReadInt32()));
        skillQuickList.Add(new Vector2Int(file.ReadInt32(), file.ReadInt32()));
        skillTreeList.Clear();
        int treeCount = file.ReadInt32();
        for (int tree = 0; tree < treeCount; tree++)
        {
            int currentTree = file.ReadInt32();
            skillTreeList.Add(currentTree, new Dictionary<int, SkillInfo>());
            int skillCount = file.ReadInt32();
            for(int skill = 0; skill < skillCount; skill++)
            {
                skillTreeList[currentTree].Add(file.ReadInt32(), new SkillInfo(file.ReadBoolean(), file.ReadInt32()));
            }
        }
        file.Close();
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
        skillPoints -= GameStorage.skillTreeList[treeID][skillID].unlockCost;
    }
}