using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//later this will be used to store players, inventories, quests etc. in between fights

public class GameStorage : MonoBehaviour {

    /*
     * 
     * Player Storage
     * 
    */

    public static List<Player> playerMasterList = new List<Player>();
    public static List<int> activePlayerList = new List<int>(){0, 1, 2, 3};

    public static int playerCurrency;

    /*
     * 
     * Map storage and access
     * 
    */ 

    const int mapXsize = 200;
    const int mapYsize = 200;

    //final top left coords chosen for the current battle map (please replace asap with something less roundabout)
    public static int trueBX;
    public static int trueBY;

    /// <summary>
    /// 1 is normally traversable land. 
    /// 2 is forest, limits movement. 
    /// 3 is water, only traversable by flying units. 
    /// 4 is rotating land, normally traversable but moves in a predefined pattern at the end of each turn carrying any units on them with it. 
    /// 5 is burning land, damages units on them after each turn. 
    /// 6 is impassable land. 
    /// 7 is breakable walls. 
    /// </summary>
    static int[,] map = new int[mapXsize, mapYsize];
    //Space 0 is for base value, space 1 is for max value. Both can be between 0-100.
    static int[,,] baseaEtherMap = new int[mapXsize, mapYsize, 2];

    //test wall
    public static GameObject testWall;

    public static Dictionary<int, Dictionary<int, Skill>> skillTreeList = new Dictionary<int, Dictionary<int, Skill>>();

    // Use this for initialization
    public static void FillStorage()
    {
        Debug.Log("Initializing storage");

        PopulateSkillTree();

        testWall = Resources.Load<GameObject>("Prefabs/Map/TestWall");

        //Instantiates temporary base players
        playerMasterList.Add(new Player(6, 10, 3, "Player1"));
        playerMasterList.Add(new Player(8, 11, 2, "Player2"));
        playerMasterList.Add(new Player(10, 10, 1, "Player3"));
        playerMasterList.Add(new Player(12, 11, 2, "Player4"));

        //Instantiate the map and aEther maps with base values
        for (int x = 0; x < mapXsize; x++)
        {
            for (int y = 0; y < mapYsize; y++)
            {
                map[x, y] = 1;
                baseaEtherMap[x, y, 0] = x + y;
                baseaEtherMap[x, y, 1] = x + y + 1;
            }
        }
        map[5, 190] = 2;
        map[6, 189] = 2;
        map[4, 195] = 2;
        map[5, 194] = 2;
        for (int x = 0; x < mapXsize; x++)
        {
            for (int y = 0; y < mapYsize; y++)
            {
                if (map[x, y] == 2)
                    Instantiate(testWall, new Vector3(x, 1, mapYsize - y - 1), Quaternion.Euler(0, 0, 0));
            }
        }
    }

    /// <summary>
    /// Grabs the structure of the surrounding map at the beginning of the battle. 
    /// If there is not enough applicable land around the center, center will be moved accordingly. 
    /// Default map size is 20x20. 
    /// </summary>
    /// <param name="centerX"> X coordinate of the center of the map, the top of the center four blocks if map size is even. </param>
    /// <param name="centerY"> Y coordinate of the center of the map, the left of the center four blocks if map size is even. </param>
    public static int[,] GrabBattleMap(int centerX, int centerY, int xSize, int ySize)
    {
        int[,] bMap = new int[xSize, ySize];

        //moves the center of the map if it would put the battleMap out of bounds
        if (centerX < (int)Mathf.Ceil(xSize / 2.0f) - 1)
            centerX = (int)Mathf.Ceil(xSize / 2.0f) - 1;
        else if (centerX > mapXsize - (int)Mathf.Floor(xSize / 2.0f) + 1)
            centerX = mapXsize - (int)Mathf.Floor(xSize / 2.0f) + 1;
        if (centerY < (int)Mathf.Ceil(ySize / 2.0f) - 1)
            centerY = (int)Mathf.Ceil(ySize / 2.0f) - 1;
        else if (centerY > mapYsize - (int)Mathf.Floor(ySize / 2.0f) + 1)
            centerY = mapYsize - (int)Mathf.Floor(ySize / 2.0f) + 1;

        trueBX = centerX - (int)Mathf.Ceil(xSize / 2.0f) + 1;
        trueBY = centerY - (int)Mathf.Ceil(ySize / 2.0f) + 1;
        Debug.Log(trueBX + " " + trueBY);

        //copies the corresponding values from the main map to the battlemap
        int tileCount = 0;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                //Debug.Log((x - ((int)Mathf.Ceil(xSize / 2.0f) - 1) + centerX) + " | " + (mapYsize - trueBY - (ySize - y)));
                //Debug.Log(x + " - (" + (int)Mathf.Ceil(xSize / 2.0f) + " - 1) + " + centerX + " | " + mapYsize + " - (" + ySize + " - (" + trueBY + " - " + y + "))");
                bMap[x, y] = map[x - ((int)Mathf.Ceil(xSize / 2.0f) - 1) + centerX, mapYsize - trueBY - (ySize - y)];
                if (bMap[x, y] == 1 || bMap[x, y] == 2 || bMap[x, y] == 4 || bMap[x, y] == 5)
                    tileCount++;
            }
        }

        //if there are less that 75 normally usable (not locked by movement type) tiles on the map the map will be moved until a suitable place is found
        if (tileCount < (xSize * ySize * 3) / 8)
        {
            int up = 0;
            int down = 0;
            int left = 0;
            int right = 0;
            for (int i = 0; i < xSize; i++)
            {
                if (bMap[i, 0] == 1 || bMap[i, 0] == 2 || bMap[i, 0] == 4 || bMap[i, 0] == 5)
                {
                    up++;
                }
                if (bMap[i, ySize - 1] == 1 || bMap[i, ySize - 1] == 2 || bMap[i, ySize - 1] == 4 || bMap[i, ySize - 1] == 5)
                {
                    down++;
                }
            }
            for (int i = 0; i < ySize; i++)
            {
                if (bMap[0, i] == 1 || bMap[0, i] == 2 || bMap[0, i] == 4 || bMap[0, i] == 5)
                {
                    left++;
                }
                if (bMap[xSize - 1, i] == 1 || bMap[xSize - 1, i] == 2 || bMap[xSize - 1, i] == 4 || bMap[xSize - 1, i] == 5)
                {
                    right++;
                }
            }
            if (up >= down && up >= left && up >= right)
            {
                bMap = GrabBattleMap(centerX, centerY - 1, xSize, ySize);
            }
            else if (left >= down && left >= right)
            {
                bMap = GrabBattleMap(centerX - 1, centerY, xSize, ySize);
            }
            else if (right >= down)
            {
                bMap = GrabBattleMap(centerX + 1, centerY, xSize, ySize);
            }
            else
            {
                bMap = GrabBattleMap(centerX, centerY + 1, xSize, ySize);
            }
        }
        return bMap;
    }

    //Passes the correct aEther level information to the battlemap
    public static int[,,] GrabaEtherMap(int topLeftX, int topLeftY, int xSize, int ySize)
    {
        int[,,] aMap = new int[xSize, ySize, 2];
        //copies the corresponding values from the main map to the battlemap
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                aMap[x, y, 0] = baseaEtherMap[topLeftX + x, topLeftY + y, 0];
                aMap[x, y, 1] = baseaEtherMap[topLeftX + x, topLeftY + y, 1];
            }
        }
        return aMap;
    }

    //skills
    public static void PopulateSkillTree()
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

    public static List<int> GetPlayerSkillList(string name)
    {
        List<int> playerSkillTrees = new List<int>();
        if (name == "Player1")
        {
            playerSkillTrees.Add(1);
            playerSkillTrees.Add(2);
            playerSkillTrees.Add(3);
            playerSkillTrees.Add(4);
        }
        if (name == "Player2")
        {
            playerSkillTrees.Add(2);
            playerSkillTrees.Add(4);
            playerSkillTrees.Add(1);
        }
        return playerSkillTrees;
    }


    /*
     * 
     * Custom Mathematical Functions
     * 
    */

    public static bool Approximately(Vector3 v1, Vector3 v2)
    {
        if (Approximately(v1.x, v2.x) && Approximately(v1.y, v2.y) && Approximately(v1.z, v2.z))
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }
    
    public static bool Approximately(Quaternion v1, Quaternion v2)
    {
        Debug.Log(v1.eulerAngles.x + " = " + v2.eulerAngles.x + " | " + v1.eulerAngles.y + " = " + v2.eulerAngles.y + " | " + v1.eulerAngles.z + " = " + v2.eulerAngles.z);
        if (Approximately(v1.eulerAngles.x, v2.eulerAngles.x) && Approximately(v1.eulerAngles.y, v2.eulerAngles.y) && Approximately(v1.eulerAngles.z, v2.eulerAngles.z))
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }

    public static bool Approximately(float f1, float f2)
    {
        if (f1 < f2 + (Mathf.Pow(1, -25)) && f1 > f2 - (Mathf.Pow(1, -25)))
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }
}