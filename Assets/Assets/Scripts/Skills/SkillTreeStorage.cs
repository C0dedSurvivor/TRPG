using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkillTreeStorage
{
    public Dictionary<int, Dictionary<int, Skill>> skillTreeList = new Dictionary<int, Dictionary<int, Skill>>();

    public SkillTreeStorage()
    {

    }

    public void PopulateSkillTree()
    {
        Dictionary<int, Skill> testSkillTree = new Dictionary<int, Skill>();

        //Skill fireball = new Skill("Fireball", 2, 1, 7, 1, 1, 0, 1);
        //fireball.addDamagePart(2, 5, 5, 0, 0);
        //testSkillTree.Add(1, fireball);

        //test skill to test damage, healing and stat changes
        Skill holyHandGrenade = new Skill("Holy Hand Grenade", 5, 1, 7, 5, 5, 0, 0);
        holyHandGrenade.addDamagePart(2, 3, 3, 0, 0);
        holyHandGrenade.addHealPart(3, 3, 3, 0, 0);
        holyHandGrenade.addStatPart(3, "atk", 5, 0, 3);
        testSkillTree.Add(1, holyHandGrenade);

        Skill firewall = new Skill("Firewall", 5, 4, 7, 1, 3, 1, 1);
        firewall.addDamagePart(2, 3, 5, 0, 0);
        firewall.addDependency(1);
        testSkillTree.Add(2, firewall);

        Skill conflagration = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        conflagration.addDamagePart(2, 10, 0, 0, 0);
        conflagration.addStatPart(2, "atk", 0, -4, 3);
        conflagration.addDependency(1);
        testSkillTree.Add(3, conflagration);

        Skill testSkill1 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill1.addDependency(3);
        testSkillTree.Add(4, testSkill1);

        Skill testSkill2 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill2.addDependency(4);
        testSkill2.addDependency(1);
        testSkillTree.Add(5, testSkill2);

        Skill testSkill3 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill3.addDependency(4);
        testSkillTree.Add(6, testSkill3);

        Skill testSkill4 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill4.addDependency(5);
        testSkillTree.Add(7, testSkill4);

        Skill testSkill5 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill5.addDependency(5);
        testSkillTree.Add(8, testSkill5);

        Skill testSkill6 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill6.addDependency(7);
        testSkillTree.Add(9, testSkill6);

        Skill testSkill7 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill7.addDependency(7);
        testSkill7.addDependency(2);
        testSkillTree.Add(10, testSkill7);

        Skill testSkill8 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill8.addDependency(7);
        testSkillTree.Add(11, testSkill8);

        Skill testSkill9 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill9.addDependency(10);
        testSkillTree.Add(12, testSkill9);


        Dictionary<int, Skill> testSkillTree2 = new Dictionary<int, Skill>();
        testSkillTree2.Add(1, holyHandGrenade);

        Dictionary<int, Skill> testSkillTree3 = new Dictionary<int, Skill>();
        testSkillTree3.Add(1, holyHandGrenade);
        testSkillTree3.Add(2, firewall);
        testSkillTree3.Add(3, conflagration);

        Dictionary<int, Skill> testSkillTree4 = new Dictionary<int, Skill>();
        testSkillTree4.Add(1, holyHandGrenade);
        testSkillTree4.Add(2, firewall);
        testSkillTree4.Add(3, conflagration);
        Skill testSkill21 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill21.addDependency(1);
        testSkillTree4.Add(4, testSkill21);

        Skill testSkill22 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill22.addDependency(2);
        testSkillTree4.Add(5, testSkill22);
        Skill testSkill23 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill23.addDependency(2);
        testSkillTree4.Add(6, testSkill23);
        Skill testSkill24 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill24.addDependency(2);
        testSkillTree4.Add(7, testSkill24);
        Skill testSkill25 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill25.addDependency(2);
        testSkillTree4.Add(8, testSkill25);

        Skill testSkill26 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill26.addDependency(3);
        testSkillTree4.Add(9, testSkill26);
        Skill testSkill27 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill27.addDependency(3);
        testSkillTree4.Add(10, testSkill27);
        Skill testSkill28 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill28.addDependency(3);
        testSkillTree4.Add(11, testSkill28);
        Skill testSkill29 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill29.addDependency(3);
        testSkillTree4.Add(12, testSkill29);

        Skill testSkill210 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill210.addDependency(4);
        testSkillTree4.Add(13, testSkill210);
        Skill testSkill211 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill211.addDependency(4);
        testSkillTree4.Add(14, testSkill211);
        Skill testSkill212 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill212.addDependency(4);
        testSkillTree4.Add(15, testSkill212);
        Skill testSkill213 = new Skill("Conflagration", 3, 2, 7, 4, 4, 1, 1);
        testSkill213.addDependency(4);
        testSkillTree4.Add(16, testSkill213);

        skillTreeList.Add(1, testSkillTree);
        skillTreeList.Add(2, testSkillTree2);
        skillTreeList.Add(3, testSkillTree3);
        skillTreeList.Add(4, testSkillTree4);
    }

    public Dictionary<int, Dictionary<int, Skill>> GetPlayerSkillList(string name)
    {
        Dictionary<int, Dictionary<int, Skill>> playerSkillTree = new Dictionary<int, Dictionary<int, Skill>>();
        if(name == "Player1")
        {
            playerSkillTree.Add(1, CopySkillTree(skillTreeList[1]));
            playerSkillTree.Add(2, CopySkillTree(skillTreeList[2]));
            playerSkillTree.Add(3, CopySkillTree(skillTreeList[3]));
            playerSkillTree.Add(4, CopySkillTree(skillTreeList[4]));
        }
        if (name == "Player2")
        {
            playerSkillTree.Add(1, CopySkillTree(skillTreeList[1]));
            playerSkillTree.Add(2, CopySkillTree(skillTreeList[2]));
            playerSkillTree.Add(3, CopySkillTree(skillTreeList[1]));
        }
        return playerSkillTree;
    }

    private Dictionary<int, Skill> CopySkillTree(Dictionary<int, Skill> oldTree)
    {
        Dictionary<int, Skill> newTree = new Dictionary<int, Skill>();
        foreach (int i in oldTree.Keys)
        {
            Skill s = new Skill(oldTree[i].name, oldTree[i].targetType, oldTree[i].aEtherCost, oldTree[i].targettingRange, oldTree[i].xRange, oldTree[i].yRange, oldTree[i].unlockCost, oldTree[i].unlockLevel);
            s.dependencies = oldTree[i].dependencies;
            s.partList = oldTree[i].partList;
            if(s.dependencies.Count == 0)
                s.unlocked = true;
            newTree.Add(i, s);
        }
        return newTree;
    }
}
