using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Player : BattlePawnBase
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

    //Points used to unlock skills
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

    /// <summary>
    /// Loads a player from a save file, or the default file if this is the first encounter
    /// </summary>
    /// <param name="path">The path of the file to load from</param>
    public Player(string path, string name = null) : base()
    {
        //Generates default player data if that player's file doesn't exist
        if (!File.Exists(path)) {
            /*
             * This will be replaced with loading the stats from a template file based on the pawn name
             */
            this.name = name;
            moveType = 4;
            stats.Add(Stats.MaxHealth, 20 + moveType);
            stats.Add(Stats.Attack, 20 + moveType);
            stats.Add(Stats.Defense, 10 + moveType);
            stats.Add(Stats.MagicAttack, 15 + moveType);
            stats.Add(Stats.MagicDefense, 15 + moveType);
            stats.Add(Stats.CritChance, 15 + moveType);
            stats.Add(Stats.MaxMove, Registry.MovementRegistry[moveType].moveSpeed);
            stats.Add(Stats.BasicAttackLifesteal, 0);
            stats.Add(Stats.SpellLifesteal, 0);
            stats.Add(Stats.BasicAttackEffectiveness, 100);
            stats.Add(Stats.SpellDamageEffectiveness, 100);
            stats.Add(Stats.BasicAttackReceptiveness, 100);
            stats.Add(Stats.SpellDamageReceptiveness, 100);
            stats.Add(Stats.HealingEffectiveness, 100);
            stats.Add(Stats.HealingReceptiveness, 100);
            cHealth = stats[Stats.MaxHealth];

            //Grab all the skill trees and skills for this pawn
            List<int> treeList = GameStorage.GetPlayerSkillList(name);
            foreach (int tree in treeList)
            {
                skillTreeList.Add(tree, new Dictionary<int, SkillInfo>());
                foreach (int skill in GameStorage.skillTreeList[tree].Keys)
                {
                    skillTreeList[tree].Add(skill, new SkillInfo());
                    if (GameStorage.skillTreeList[tree][skill].dependencies.Count == 0)
                        skillTreeList[tree][skill].unlocked = true;
                }
            }

            //Divides by two to test healing
            //cHealth = GetEffectiveStat(Stats.MaxHealth) / 2;
            equippedWeapon = new Equippable("Dagger");
            skillQuickList.Add(new Vector2Int(1, 1));
            skillQuickList.Add(new Vector2Int(1, 2));
            skillQuickList.Add(new Vector2Int(1, 3));
        }
        else
        {
            Stream inStream = File.OpenRead(path);
            BinaryReader file = new BinaryReader(inStream);
            level = file.ReadInt32();
            exp = file.ReadInt32();
            skillPoints = file.ReadInt32();
            moveType = file.ReadInt32();
            stats[Stats.Attack] = file.ReadInt32();
            attackGrowthType = file.ReadString();
            stats[Stats.Defense] = file.ReadInt32();
            defenseGrowthType = file.ReadString();
            stats[Stats.MagicAttack] = file.ReadInt32();
            mAttackGrowthType = file.ReadString();
            stats[Stats.MagicDefense] = file.ReadInt32();
            mDefenseGrowthType = file.ReadString();
            stats[Stats.CritChance] = file.ReadInt32();
            cHealth = file.ReadInt32();
            stats[Stats.MaxHealth] = file.ReadInt32();
            stats[Stats.MaxMove] = Registry.MovementRegistry[moveType].moveSpeed;
            healthGrowthType = file.ReadString();
            for (int i = 0; i < equipment.Length; i++)
            {
                equipment[i] = new Equippable(file.ReadString());
            }
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
                for (int skill = 0; skill < skillCount; skill++)
                {
                    skillTreeList[currentTree].Add(file.ReadInt32(), new SkillInfo(file.ReadBoolean(), file.ReadInt32()));
                }
            }
            file.Close();
            //For testing healing
            //cHealth /= 2;
            //For testing specific weapons
            //equippedWeapon = "Demonic Sword";
        }
    }

    /// <summary>
    /// Saves the player data to its respective file
    /// </summary>
    /// <param slot="slot">The save slot to save to</param>
    public void SavePlayer(int slot)
    {
        Debug.Log("Saving the player");
        Stream outStream = File.OpenWrite("Assets/Resources/Storage/Slot" + slot + "/Players/" + name + ".data");
        BinaryWriter file = new BinaryWriter(outStream);
        file.Write(level);
        file.Write(exp);
        file.Write(skillPoints);
        file.Write(moveType);
        file.Write(stats[Stats.Attack]);
        file.Write(attackGrowthType);
        file.Write(stats[Stats.Defense]);
        file.Write(defenseGrowthType);
        file.Write(stats[Stats.MagicAttack]);
        file.Write(mAttackGrowthType);
        file.Write(stats[Stats.MagicDefense]);
        file.Write(mDefenseGrowthType);
        file.Write(stats[Stats.CritChance]);
        file.Write(cHealth);
        file.Write(stats[Stats.MaxHealth]);
        file.Write(healthGrowthType);
        foreach (Equippable equipped in equipment)
        {
            file.Write(equipped == null ? "null" : equipped.Name);
        }
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

    /// <summary>
    /// Equips the new item, returning the old one so it can be returned to the inventory. 
    /// </summary>
    public Equippable EquipItem(Equippable newEquippable, int slot) {
        Equippable item = equipment[slot];
        equipment[slot] = newEquippable;
		return item;
    }

    /// <summary>
    /// Returns the item equipped in inventory slot "slot". 
    /// </summary>
    public Equippable GetEquipped(int slot)
    {
        return equipment[slot];
    }

    public void GainExp(int e){
		exp += e;
		CheckForLevel();
	}
    
    /// <summary>
    /// Sees if you have enough EXP to level up and does the level up if so
    /// </summary>
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
					stats[Stats.Attack]++;

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
                    stats[Stats.Defense]++;

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
                    stats[Stats.MagicAttack]++;

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
                    stats[Stats.MagicDefense]++;

			CheckForLevel();
		}
	}
    
    /// <summary>
    /// Gets how much EXP is needed to get to the next level
    /// </summary>
    /// <returns>The amount of EXP needed to level up</returns>
	private int GetExpPerLevel(){
		return -20 + level * 50;
    }

    public void EndOfMatch()
    {
        tempStats = null;
        statusList.EndOfMatch();
    }
    
    /// <summary>
    /// Unlocks a skill and subtracts the corresponding cost from the skill points total
    /// </summary>
    /// <param name="treeID">What tree this skill exists in</param>
    /// <param name="skillID">What skill in that tree is being unlocked</param>
    public void UnlockSkill(int treeID, int skillID)
    {
        skillTreeList[treeID][skillID].unlocked = true;
        skillPoints -= GameStorage.skillTreeList[treeID][skillID].unlockCost;
    }
}