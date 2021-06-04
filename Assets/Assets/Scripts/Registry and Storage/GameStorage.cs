using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameStorage : MonoBehaviour
{
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
    public static Terrain mapTerrain;

    //final top left coords chosen for the current battle map (please replace asap with something less roundabout)
    public static int trueBX;
    public static int trueBY;

    static int[,] map = new int[mapXsize, mapYsize];
    //Space 0 is for base value, space 1 is for max value. Both can be between 0-100.
    static int[,,] baseaEtherMap = new int[mapXsize, mapYsize, 2];

    //Test wall
    public static GameObject testWall;

    public static MapPlayerScript mapPlayer;

    /// <summary>
    /// ONLY FOR TESTING
    /// Grabs testing map, fills registry, and loads a blank save
    /// </summary>
    void Awake()
    {
        mapTerrain = GameObject.FindGameObjectWithTag("Ground").GetComponent<Terrain>();
        mapPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<MapPlayerScript>();

        //If the registry hasn't already been filled
        //Only for testing game while bypassing title screen
        if (Registry.MovementRegistry.Count == 0)
        {
            Registry.FillRegistry();
            QuestManager.AcceptQuest(0);
            LoadSaveData(-1);
        }
    }

    public static void LoadSaveData(int slot)
    {
        if (slot != -1)
        {
            //Load players
            string[] playerFiles = Directory.GetFiles("Assets/Resources/Storage/Slot" + slot + "/Players");
            foreach (string playerData in playerFiles)
            {
                playerMasterList.Add(new Player(playerData));
            }
        }

        playerMasterList.Add(new Player("falsetemppath", "Player1"));
        playerMasterList.Add(new Player("falsetemppath", "Player2"));
        playerMasterList.Add(new Player("falsetemppath", "Player3"));
        playerMasterList.Add(new Player("falsetemppath", "Player4"));

        //Load map
        for (int x = 0; x < mapXsize; x++)
        {
            for (int y = 0; y < mapYsize; y++)
            {
                //Normal Tiles in the base library
                map[x, y] = 1;
                baseaEtherMap[x, y, 0] = x + y;
                baseaEtherMap[x, y, 1] = x + y + 1;
            }
        }
        //Completely impassable tiles
        map[5, 190] = 0;
        map[6, 189] = 0;
        map[4, 195] = 0;
        map[5, 194] = 0;

        //Loads inventory
        Inventory.LoadInventory(slot);
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
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                bMap[x, y] = map[x - ((int)Mathf.Ceil(xSize / 2.0f) - 1) + centerX, mapYsize - trueBY - (ySize - y)];
            }
        }
        return bMap;
    }

    /// <summary>
    /// Passes the correct aEther level information to the battlemap
    /// </summary>
    public static int[,,] GrabaEtherMap(int bottomLeftX, int bottomLeftY, int xSize, int ySize)
    {
        int[,,] aMap = new int[xSize, ySize, 2];
        //Copies the corresponding values from the main map to the battlemap
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                aMap[x, y, 0] = baseaEtherMap[bottomLeftX + x, bottomLeftY + y, 0];
                aMap[x, y, 1] = baseaEtherMap[bottomLeftX + x, bottomLeftY + y, 1];
            }
        }
        return aMap;
    }

    /// <summary>
    /// Returns the ID of all skill trees a given pawn should have access to
    /// <see cref="BattlePawnBase"/>
    /// </summary>
    /// <param name="name">The name of the pawn</param>
    /// <returns>A list of skill tree IDs</returns>
    public static List<int> GetPlayerSkillList(string name)
    {
        List<int> playerSkillTrees = new List<int>();
        switch (name)
        {
            case "Player1":
                playerSkillTrees.Add(0);
                playerSkillTrees.Add(1);
                playerSkillTrees.Add(2);
                playerSkillTrees.Add(3);
                break;
            case "Player2":
                playerSkillTrees.Add(0);
                playerSkillTrees.Add(1);
                playerSkillTrees.Add(2);
                playerSkillTrees.Add(3);
                break;
            case "Player3":
                playerSkillTrees.Add(0);
                playerSkillTrees.Add(1);
                playerSkillTrees.Add(2);
                playerSkillTrees.Add(3);
                break;
            case "Player4":
                playerSkillTrees.Add(0);
                playerSkillTrees.Add(1);
                playerSkillTrees.Add(2);
                playerSkillTrees.Add(3);
                break;
        }
        return playerSkillTrees;
    }

    /// <summary>
    /// Saves all of the important data across the game
    /// </summary>
    /// <param slot="slot">The save slot to save to</param>
    public static void SaveAll(int slot)
    {
        //Saves each pawn's stats and gear
        foreach (Player p in playerMasterList)
        {
            p.SavePlayer(slot);
        }
        Inventory.SaveInventory(slot);
        //Saves any data not specifically in any other file
        Stream outStream = File.OpenWrite("Assets/Resources/Storage/Slot" + slot + "/Player.data");
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
        foreach (int i in activePlayerList)
        {
            file.Write(i);
        }
        file.Close();
    }

    /// <summary>
    /// Loads all globally relevant data
    /// Should be the last thing loaded
    /// </summary>
    public static void Load(int slot)
    {
        if (File.Exists("Assets/Resources/Storage/Slot" + slot + "/Player.data"))
        {
            Stream inStream = File.OpenRead("Assets/Resources/Storage/Slot" + slot + "/Player.data");
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
            for (int i = 0; i < activeAmount; i++)
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
        return Approximately(v1.eulerAngles, v2.eulerAngles);
    }

    /// <summary>
    /// Translates a stat enum value to a cleaner string version for textual output
    /// </summary>
    /// <param name="stat">Stat to translate</param>
    /// <returns>String version of the stat. Any time an uppercase letter is encountered past the first a space is added for readability</returns>
    public static string StatToString(Stats stat)
    {
        string s = stat.ToString();
        for (int i = 1; i < s.Length; i++)
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