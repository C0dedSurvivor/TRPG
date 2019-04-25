using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum BattleTiles
{
    Impassable,
    Normal,
    //Limits movement of some units
    Forest,
    //Only some movement types can pass over this
    Water,
    //Damage pawns that pass over and end their turn on this tile
    Burning,
    //Impassable normally, but can be broken to make it passable
    Breakable,
    //Moves any pawns that end their turns on these in a set direction
    MoveUp,
    MoveLeft,
    MoveDown,
    MoveRight,
    MoveRandom
}

public class GameStorage : MonoBehaviour {

    /*
     * 
     * Player Storage
     * 
    */

    public static List<Player> playerMasterList = new List<Player>();
    public static List<int> activePlayerList = new List<int>() { 0, 1, 2, 3 };

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

    static int[,] map = new int[mapXsize, mapYsize];
    //Space 0 is for base value, space 1 is for max value. Both can be between 0-100.
    static int[,,] baseaEtherMap = new int[mapXsize, mapYsize, 2];

    //Test wall
    public static GameObject testWall;

    //List of all skill trees
    public static Dictionary<int, Dictionary<int, Skill>> skillTreeList = new Dictionary<int, Dictionary<int, Skill>>();

    // Use this for initialization
    public static void FillStorage()
    {
        Debug.Log("Initializing storage");

        PopulateSkillTree();

        testWall = Resources.Load<GameObject>("Prefabs/Map/TestWall");

        //Instantiates temporary base players
        playerMasterList.Add(new Player(6, 10, 3, "Player1"));
        playerMasterList.Add(new Player(8, 11, 3, "Player2"));
        playerMasterList.Add(new Player(10, 10, 3, "Player3"));
        playerMasterList.Add(new Player(12, 11, 3, "Player4"));

        //Instantiate the map and aEther maps with base values
        for (int x = 0; x < mapXsize; x++)
        {
            for (int y = 0; y < mapYsize; y++)
            {
                map[x, y] = (int)BattleTiles.Normal;
                baseaEtherMap[x, y, 0] = x + y;
                baseaEtherMap[x, y, 1] = x + y + 1;
            }
        }
        map[5, 190] = (int)BattleTiles.Impassable;
        map[6, 189] = (int)BattleTiles.Impassable;
        map[4, 195] = (int)BattleTiles.Impassable;
        map[5, 194] = (int)BattleTiles.Impassable;
        for (int x = 0; x < mapXsize; x++)
        {
            for (int y = 0; y < mapYsize; y++)
            {
                if (map[x, y] == (int)BattleTiles.Impassable)
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

        //Moves the center of the map if it would put the battleMap out of bounds
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

        //Copies the corresponding values from the main map to the battlemap
        int tileCount = 0;
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                //Debug.Log((x - ((int)Mathf.Ceil(xSize / 2.0f) - 1) + centerX) + " | " + (mapYsize - trueBY - (ySize - y)));
                //Debug.Log(x + " - (" + (int)Mathf.Ceil(xSize / 2.0f) + " - 1) + " + centerX + " | " + mapYsize + " - (" + ySize + " - (" + trueBY + " - " + y + "))");
                bMap[x, y] = map[x - ((int)Mathf.Ceil(xSize / 2.0f) - 1) + centerX, mapYsize - trueBY - (ySize - y)];
                if (IsGenericPassableTile(bMap[x, y]))
                    tileCount++;
            }
        }

        //If there are less that 75 normally usable (not locked by movement type) tiles on the map the map will be moved until a suitable place is found
        if (tileCount < (xSize * ySize * 3) / 8)
        {
            int up = 0;
            int down = 0;
            int left = 0;
            int right = 0;
            for (int i = 0; i < xSize; i++)
            {
                if (IsGenericPassableTile(bMap[i, 0]))
                {
                    up++;
                }
                if (IsGenericPassableTile(bMap[i, ySize - 1]))
                {
                    down++;
                }
            }
            for (int i = 0; i < ySize; i++)
            {
                if (IsGenericPassableTile(bMap[0, i]))
                {
                    left++;
                }
                if (IsGenericPassableTile(bMap[xSize - 1, i]))
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

    /// <summary>
    /// Checks if the given int represents a tile passable by the basic movement type
    /// </summary>
    /// <param name="tile">The int to check</param>
    public static bool IsGenericPassableTile(int tile)
    {
        return Registry.MovementRegistry[2].passableTiles.Contains((BattleTiles)tile);
    }
    
    /// <summary>
    /// Passes the correct aEther level information to the battlemap
    /// </summary>
    public static int[,,] GrabaEtherMap(int topLeftX, int topLeftY, int xSize, int ySize)
    {
        int[,,] aMap = new int[xSize, ySize, 2];
        //Copies the corresponding values from the main map to the battlemap
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

    /// <summary>
    /// Populates all of the skill trees with their corresponding skills and adds them to the master list
    /// </summary>
    public static void PopulateSkillTree()
    {
        Dictionary<int, Skill> testSkillTree = new Dictionary<int, Skill>();

        //Skill fireball = new Skill("Fireball", 2, 1, 7, 1, 1, 0, 1);
        //fireball.AddDamagePart(2, 5, 5, 0, 0);
        //testSkillTree.Add(1, fireball);

        //Adds all of the skills to the trees

        //test skill to test damage, healing and stat changes
        Skill holyHandGrenade = new Skill("Holy Hand Grenade", TargettingType.AllInRange, 1, 7, 5, 5, 0, 0);
        holyHandGrenade.AddDamagePart(TargettingType.Enemy, DamageType.Magical, 3, 3, 0, 0);
        holyHandGrenade.AddHealPart(TargettingType.Ally, 3, 3, 0, 0);
        holyHandGrenade.AddStatPart(TargettingType.Ally, Stats.Attack, 5, 0, 3);
        testSkillTree.Add(1, holyHandGrenade);

        Skill firewall = new Skill("Firewall", TargettingType.AllInRange, 4, 7, 1, 3, 1, 1);
        firewall.AddDamagePart(TargettingType.Enemy, DamageType.Magical, 3, 5, 0, 0);
        firewall.AddDependency(1);
        testSkillTree.Add(2, firewall);

        Skill conflagration = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        conflagration.AddDamagePart(TargettingType.Enemy, DamageType.Magical, 10, 0, 0, 0);
        conflagration.AddStatPart(TargettingType.Enemy, Stats.Attack, 0, -4, 3);
        conflagration.AddDependency(1);
        testSkillTree.Add(3, conflagration);

        Skill testSkill1 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill1.AddDependency(3);
        testSkillTree.Add(4, testSkill1);

        Skill testSkill2 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill2.AddDependency(4);
        testSkill2.AddDependency(1);
        testSkillTree.Add(5, testSkill2);

        Skill testSkill3 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill3.AddDependency(4);
        testSkillTree.Add(6, testSkill3);

        Skill testSkill4 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill4.AddDependency(5);
        testSkillTree.Add(7, testSkill4);

        Skill testSkill5 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill5.AddDependency(5);
        testSkillTree.Add(8, testSkill5);

        Skill testSkill6 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill6.AddDependency(7);
        testSkillTree.Add(9, testSkill6);

        Skill testSkill7 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill7.AddDependency(7);
        testSkill7.AddDependency(2);
        testSkillTree.Add(10, testSkill7);

        Skill testSkill8 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill8.AddDependency(7);
        testSkillTree.Add(11, testSkill8);

        Skill testSkill9 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill9.AddDependency(10);
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
        Skill testSkill21 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill21.AddDependency(1);
        testSkillTree4.Add(4, testSkill21);

        Skill testSkill22 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill22.AddDependency(2);
        testSkillTree4.Add(5, testSkill22);
        Skill testSkill23 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill23.AddDependency(2);
        testSkillTree4.Add(6, testSkill23);
        Skill testSkill24 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill24.AddDependency(2);
        testSkillTree4.Add(7, testSkill24);
        Skill testSkill25 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill25.AddDependency(2);
        testSkillTree4.Add(8, testSkill25);

        Skill testSkill26 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill26.AddDependency(3);
        testSkillTree4.Add(9, testSkill26);
        Skill testSkill27 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill27.AddDependency(3);
        testSkillTree4.Add(10, testSkill27);
        Skill testSkill28 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill28.AddDependency(3);
        testSkillTree4.Add(11, testSkill28);
        Skill testSkill29 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill29.AddDependency(3);
        testSkillTree4.Add(12, testSkill29);

        Skill testSkill210 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill210.AddDependency(4);
        testSkillTree4.Add(13, testSkill210);
        Skill testSkill211 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill211.AddDependency(4);
        testSkillTree4.Add(14, testSkill211);
        Skill testSkill212 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill212.AddDependency(4);
        testSkillTree4.Add(15, testSkill212);
        Skill testSkill213 = new Skill("Conflagration", TargettingType.Enemy, 2, 7, 4, 4, 1, 1);
        testSkill213.AddDependency(4);
        testSkillTree4.Add(16, testSkill213);

        //Adds all of the trees to the master list
        skillTreeList.Add(1, testSkillTree);
        skillTreeList.Add(2, testSkillTree2);
        skillTreeList.Add(3, testSkillTree3);
        skillTreeList.Add(4, testSkillTree4);
    }

    /// <summary>
    /// Returns the ID of all skill trees a given pawn should have access to
    /// </summary>
    /// <param name="name">The name of the pawn</param>
    /// <returns>A list of skill tree IDs</returns>
    public static List<int> GetPlayerSkillList(string name)
    {
        List<int> playerSkillTrees = new List<int>();
        switch (name)
        {
            case "Player1":
                playerSkillTrees.Add(1);
                playerSkillTrees.Add(2);
                playerSkillTrees.Add(3);
                playerSkillTrees.Add(4);
                break;
            case "Player2":
                playerSkillTrees.Add(2);
                playerSkillTrees.Add(4);
                playerSkillTrees.Add(1);
                break;
        }
        return playerSkillTrees;
    }

    /// <summary>
    /// Allows saving from the map instance of this class
    /// Is a workaround to allow a button press to call this function
    /// </summary>
    public void Save()
    {
        SaveAll();
    }

    /// <summary>
    /// Saves all of the important data across the game
    /// </summary>
    private static void SaveAll()
    {
        //Saves each pawn's stats and gear
        foreach(Player p in playerMasterList)
        {
            p.SavePlayer();
        }
        Inventory.SaveInventory();
        //Saves any data not specifically in any other file
        Stream outStream = File.OpenWrite("Assets/Resources/Storage/Player.data");
        BinaryWriter file = new BinaryWriter(outStream);
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        //Saves the player's position and rotation
        file.Write(player.position.x);
        file.Write(player.position.y);
        file.Write(player.position.z);
        file.Write(player.rotation.eulerAngles.x);
        file.Write(player.rotation.eulerAngles.y);
        file.Write(player.rotation.eulerAngles.z);
        //Saves the player's camera position, rotation and FoV
        Transform camera = player.GetChild(0);
        file.Write(camera.localPosition.x);
        file.Write(camera.localPosition.y);
        file.Write(camera.localPosition.z);
        file.Write(camera.rotation.eulerAngles.x);
        file.Write(camera.rotation.eulerAngles.y);
        file.Write(camera.rotation.eulerAngles.z);
        file.Write(camera.GetComponent<Camera>().fieldOfView);
        file.Write(playerCurrency);
        //Saves the current active player list (The ID's of the players set up to be used in battle)
        file.Write(activePlayerList.Count);
        foreach(int i in activePlayerList)
        {
            file.Write(i);
        }
        file.Close();
    }

    /// <summary>
    /// Loads all globally relevant data
    /// Should be the last thing loaded
    /// </summary>
    public static void Load()
    {
        if (File.Exists("Assets/Resources/Storage/Player.data"))
        {
            Stream inStream = File.OpenRead("Assets/Resources/Storage/Player.data");
            BinaryReader file = new BinaryReader(inStream);
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            player.SetPositionAndRotation(new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle()), Quaternion.Euler(file.ReadSingle(), file.ReadSingle(), file.ReadSingle()));
            Transform camera = player.GetChild(0);
            camera.localPosition = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());
            camera.rotation = Quaternion.Euler(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());
            camera.GetComponent<Camera>().fieldOfView = file.ReadSingle();
            playerCurrency = file.ReadInt32();
            int activeAmount = file.ReadInt32();
            activePlayerList.Clear();
            for(int i = 0; i < activeAmount; i++)
            {
                activePlayerList.Add(file.ReadInt32());
            }
            file.Close();
        }
    }

    /*
     * 
     * Custom Helper Functions
     * 
    */

    /// <summary>
    /// Tests to see if two variables have approximately the same value
    /// </summary>
    public static bool Approximately(float f1, float f2)
    {
        return f1 < f2 + Mathf.Pow(10, -3) && f1 > f2 - Mathf.Pow(10, -3);
    }

    public static bool Approximately(Vector3 v1, Vector3 v2)
    {
        return Approximately(v1.x, v2.x) && Approximately(v1.y, v2.y) && Approximately(v1.z, v2.z);
    }
    
    public static bool Approximately(Quaternion v1, Quaternion v2)
    {
        //Debug.Log(v1.eulerAngles.x + " = " + v2.eulerAngles.x + " | " + v1.eulerAngles.y + " = " + v2.eulerAngles.y + " | " + v1.eulerAngles.z + " = " + v2.eulerAngles.z);
        return Approximately(v1.eulerAngles, v2.eulerAngles);
    }

    public static string StatToString(Stats stat)
    {
        string s = stat.ToString();
        for(int i = 1; i < s.Length; i++)
        {
            if (char.IsUpper(s[i]))
            {
                s = s.Substring(0, i) + " " + s.Substring(i);
                i++;
            }
        }
        return s;
    }
}