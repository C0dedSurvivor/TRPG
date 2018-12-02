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

    public static SkillTreeStorage skills = new SkillTreeStorage();

    // Use this for initialization
    public static void FillStorage()
    {
        Debug.Log("Initializing storage");

        skills.PopulateSkillTree();

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
                Debug.Log((x - ((int)Mathf.Ceil(xSize / 2.0f) - 1) + centerX) + " | " + (mapYsize - trueBY - (ySize - y)));
                Debug.Log(x + " - (" + (int)Mathf.Ceil(xSize / 2.0f) + " - 1) + " + centerX + " | " + mapYsize + " - (" + ySize + " - (" + trueBY + " - " + y + "))");
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


    /*
     * 
     * Custom Mathematical Functions
     * 
    */

    public static bool Approximately(Vector3 v1, Vector3 v2)
    {
        if (Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z))
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }

    //the default Approximately didn't like me, hense fuckYouEpsilon
    public static bool Approximately(Quaternion v1, Quaternion v2)
    {
        bool fuckYouEpsilon = (Mathf.Approximately(v1.eulerAngles.y, v2.eulerAngles.y) || Mathf.Abs(v1.eulerAngles.y) < 0.000001f);
        Debug.Log(v1.eulerAngles.x + " = " + v2.eulerAngles.x + " | " + v1.eulerAngles.y + " = " + v2.eulerAngles.y + " | " + v1.eulerAngles.z + " = " + v2.eulerAngles.z);
        if (Mathf.Approximately(v1.eulerAngles.x, v2.eulerAngles.x) && fuckYouEpsilon && Mathf.Approximately(v1.eulerAngles.z, v2.eulerAngles.z))
        {
            Debug.Log("true");
            return true;
        }
        Debug.Log("false");
        return false;
    }
}