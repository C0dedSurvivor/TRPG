using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The events that could cause an effect to trigger
/// </summary>
public enum EffectTriggers
{
    FallBelow25Percent,
    FallBelow50Percent,
    RiseAbove25Percent,
    RiseAbove50Percent,
    TakeDamage,
    DealDamage,
    TakePhysicalDamage,
    LeavePhysicalDamage,
    TakeMagicDamage,
    LeaveMagicDamage,
    BasicAttack,
    SpellCast,
    KillAnEnemy,
    GettingHealed,
    Healing,
    StartOfMatch,
    StartOfTurn,
    EndOfTurn,
    EndOfMatch
}

/// <summary>
/// Stores a possible enemy move
/// </summary>
public struct EnemyMove
{
    public Vector2Int movePosition;
    public Vector2Int attackPosition;
    public int priority;
    public int reasonPriority;
    public bool yFirst;

    public EnemyMove(int x, int y, int p, int rP, bool xF)
    {
        movePosition = new Vector2Int(x, y);
        attackPosition = new Vector2Int(-1, -1);
        priority = p;
        reasonPriority = rP;
        yFirst = xF;
    }
    public EnemyMove(int x, int y, int aX, int aY, int p, int rP, bool xF)
    {
        movePosition = new Vector2Int(x, y);
        attackPosition = new Vector2Int(aX, aY);
        priority = p;
        reasonPriority = rP;
        yFirst = xF;
    }

    /// <summary>
    /// Determines which move has a higher priority
    /// </summary>
    /// <param name="m">The move to check this one against</param>
    public int CompareTo(EnemyMove m)
    {
        if (priority > m.priority)
        {
            return -1;
        }
        else if (priority < m.priority)
        {
            return 1;
        }
        else
        {
            if (reasonPriority > m.reasonPriority)
            {
                return -1;
            }
            else if (reasonPriority < m.reasonPriority)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}

/// <summary>
/// Stores basic animation data
/// </summary>
public struct MoveQueue
{
    public int entityID;
    public Vector2 relativeMove;

    public MoveQueue(int e, Vector2 relativeMoveCoords)
    {
        entityID = e;
        relativeMove = relativeMoveCoords;
    }
}

public enum BattleState
{
    None,
    BattleSetup,
    Swap,
    Player,
    MovePlayer,
    Attack,
    Enemy,
    MoveEnemy,
    ReturnCamera
}

public class Battle : MonoBehaviour
{
    //Prefabs
    public GameObject EnemyBattleModelPrefab;
    public GameObject PlayerBattleModelPrefab;
    public GameObject MoveMarkerPrefab;
    public GameObject CameraPrefab;
    public GameObject skillCastConfirmMenu;

    public GameObject battleTile;
    public Vector2Int topLeft;

    public bool showDanger = false;
    public bool showaEther = false;

    //Battle state for the finite state machine
    public static BattleState battleState = BattleState.None;
    //Whether or not the players can swap positions, only true if no one has moved yet
    public bool canSwap;

    //Declares the map size, unchanged post initialization. Default is 20x20, camera will not change view to accomodate larger currently
    int mapSizeX;
    int mapSizeY;
    //Affects what the enemies take into account when making their moves, see MoveEnemies() for more information
    int difficulty;

    //Stores the physical tiles generated in the world to detect and interpret player input
    GameObject[,] tileList;
    //Stores the data representation of the current chunk of the world, dictates where participants can move
    int[,] battleMap;
    //Stores the aEther levels of the area, slot 0 = current level, slot 1 = max level
    int[,,] aEtherMap;
    //Stores the enemy data
    public Enemy[] enemyList;
    //Stores the visual representation of the participants
    public GameObject[] playerModels = new GameObject[4];
    public GameObject[] enemyModels = new GameObject[4];
    //This is a camera
    private GameObject battleCamera;
    public GameObject mapPlayer;

    //-1 means nothing selected
    public int selectedPlayer = -1;
    public int selectedEnemy = -1;
    public int hoveredSpell = -1;
    public int selectedSpell = -1;
    private int turn = 1;
    public Vector2Int selectedMoveSpot = new Vector2Int(-1, -1);
    //This displays how the pawn would move when a move is selected
    public GameObject moveMarker;

    //Stores where the pieces need to be moved to to match up with where they need to be
    private Queue<MoveQueue> playerAnimMoves = new Queue<MoveQueue>();
    private Queue<MoveQueue> enemyAnimMoves = new Queue<MoveQueue>();
    //How fast the pieces move
    public float animSpeed = 0.1f;

    public int movingEnemy;
    private EnemyMove chosenMove;
    private bool moveYFirst;

    //The initial and final positions for animations involving the camera
    private Vector3 cameraInitPos;
    private Quaternion cameraInitRot;
    private Vector3 cameraFinalPos;
    private Quaternion cameraFinalRot;

    //Whether a change has been made that would affect the states of one or more tiles
    //Keeps updateTiles from being called every frame
    private bool updateTilesThisFrame = false;

    private Vector2Int spellCastPosition = new Vector2Int(-1, -1);

    // Use this for initialization
    void Awake()
    {
        Registry.FillRegistry();
        GameStorage.FillStorage();
        Inventory.LoadInventory();
    }

    /// <summary>
    /// Sets up all of the variables and prefabs needed during the battle
    /// </summary>
    /// <param name="centerX">Center x position of the board</param>
    /// <param name="centerY">Center y position of the board</param>
    /// <param name="mainCamera">The current main camera, most likely the over-the-shoulder camera on the player</param>
    /// <param name="xSize">Board width</param>
    /// <param name="ySize">Board height</param>
    public void StartBattle(int centerX, int centerY, Transform mainCamera, int xSize = 20, int ySize = 20)
    {
        //removes whatever is left of the previous battle
        ExpungeAll();
        cameraInitPos = mainCamera.position;
        cameraInitRot = mainCamera.rotation;
        //sets the map size
        mapSizeX = xSize;
        mapSizeY = ySize;
        //generates enemies
        enemyList = new Enemy[4];
        enemyList[0] = new Enemy(5, 5, 3, 5, 5);
        enemyList[1] = new Enemy(10, 5, 2, 5, 5);
        enemyList[2] = new Enemy(12, 5, 2, 5, 5);
        enemyList[3] = new Enemy(14, 5, 5, 5, 5);
        //grabs the map layout
        battleMap = GameStorage.GrabBattleMap(centerX, centerY, xSize, ySize);
        //finds the top left corner of the current map
        topLeft = new Vector2Int(GameStorage.trueBX, GameStorage.trueBY);
        //generates the visible tile map
        tileList = new GameObject[mapSizeX, mapSizeY];
        GenerateTileMap(topLeft.x, topLeft.y);
        //grabs the aEther map
        aEtherMap = GameStorage.GrabaEtherMap(topLeft.x, topLeft.y, xSize, ySize);
        //adds other things
        moveMarker = Instantiate(MoveMarkerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
        moveMarker.SetActive(false);
        //make the camera
        battleCamera = Instantiate(CameraPrefab);
        cameraFinalRot = battleCamera.transform.rotation;
        battleCamera.transform.SetPositionAndRotation(cameraInitPos, cameraInitRot);
        //Calculates where the camera needs to end up to frame the battle correctly
        cameraFinalPos = new Vector3(topLeft.x + (xSize / 2), 19, topLeft.y + (ySize / 2));
        battleCamera.GetComponent<Camera>().tag = "MainCamera";
        //Moves the player and enemy models into their correct position and sets up default values
        for (int i = 0; i < GameStorage.activePlayerList.Count; i++)
        {
            GameStorage.playerMasterList[GameStorage.activePlayerList[i]].position = new Vector2Int(6 + 2 * i, 10 + i % 2);
            GameStorage.playerMasterList[GameStorage.activePlayerList[i]].moved = false;
            GameStorage.playerMasterList[GameStorage.activePlayerList[i]].StartOfMatch();
            CheckEventTriggers(GameStorage.playerMasterList[GameStorage.activePlayerList[i]], EffectTriggers.StartOfMatch);
            CheckEventTriggers(GameStorage.playerMasterList[GameStorage.activePlayerList[i]], EffectTriggers.StartOfTurn);
            playerModels[i] = Instantiate(PlayerBattleModelPrefab);
            playerModels[i].transform.position = new Vector3(GameStorage.playerMasterList[GameStorage.activePlayerList[i]].position.x + topLeft.x, 1, (mapSizeY - 1) - GameStorage.playerMasterList[GameStorage.activePlayerList[i]].position.y + topLeft.y);
        }
        for (int i = 0; i < enemyList.Length; i++)
        {
            enemyList[i].StartOfMatch();
            CheckEventTriggers(enemyList[i], EffectTriggers.StartOfMatch);
            CheckEventTriggers(enemyList[i], EffectTriggers.StartOfTurn);
            enemyModels[i] = Instantiate(EnemyBattleModelPrefab);
            enemyModels[i].transform.position = new Vector3(enemyList[i].position.x + topLeft.x, 1, (mapSizeY - 1) - enemyList[i].position.y + topLeft.y);
        }
        skillCastConfirmMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        battleState = BattleState.BattleSetup;
    }

    /// <summary>
    /// Update is called once per frame
    /// Controls the battle's finite state machine and player input
    /// </summary>
    void Update()
    {
        if (skillCastConfirmMenu.activeSelf == false)
        {
            switch (battleState)
            {
                case BattleState.None:
                    break;
                case BattleState.BattleSetup:
                    //Debug.Log(battleCamera.transform.position + "|" + battleCamera.transform.rotation.eulerAngles);
                    battleCamera.transform.position = (Vector3.Lerp(battleCamera.transform.position, cameraFinalPos, 0.1f));
                    battleCamera.transform.rotation = (Quaternion.Lerp(battleCamera.transform.rotation, cameraFinalRot, 0.1f));
                    if (GameStorage.Approximately(battleCamera.transform.position, cameraFinalPos) && GameStorage.Approximately(battleCamera.transform.rotation, cameraFinalRot))
                    {
                        battleState = BattleState.Player;
                        battleCamera.transform.position = cameraFinalPos;
                        battleCamera.transform.rotation = cameraFinalRot;
                        //sets up the background variables
                        canSwap = true;
                    }
                    break;
                case BattleState.ReturnCamera:
                    Debug.Log(mapPlayer.GetComponentInChildren<Camera>().transform.position + "|" + mapPlayer.GetComponentInChildren<Camera>().transform.rotation.eulerAngles);
                    mapPlayer.GetComponentInChildren<Camera>().transform.position = (Vector3.Lerp(mapPlayer.GetComponentInChildren<Camera>().transform.position, cameraFinalPos, 0.1f));
                    mapPlayer.GetComponentInChildren<Camera>().transform.rotation = (Quaternion.Lerp(mapPlayer.GetComponentInChildren<Camera>().transform.rotation, cameraFinalRot, 0.1f));
                    if (GameStorage.Approximately(mapPlayer.GetComponentInChildren<Camera>().transform.position, cameraFinalPos) && GameStorage.Approximately(mapPlayer.GetComponentInChildren<Camera>().transform.rotation, cameraFinalRot))
                    {
                        battleState = BattleState.None;
                        mapPlayer.GetComponentInChildren<Camera>().transform.position = cameraFinalPos;
                        mapPlayer.GetComponentInChildren<Camera>().transform.rotation = cameraFinalRot;
                    }
                    break;
                case BattleState.MovePlayer:
                    MoveQueue move = playerAnimMoves.Peek();
                    Vector2 initPlayerPos = new Vector2(GameStorage.playerMasterList[GameStorage.activePlayerList[move.entityID]].position.x + topLeft.x, (mapSizeY - 1) - GameStorage.playerMasterList[GameStorage.activePlayerList[move.entityID]].position.y + topLeft.y);
                    playerModels[move.entityID].transform.Translate(Vector2.Lerp(Vector2.zero, move.relativeMove, animSpeed).x, 0, Vector2.Lerp(Vector2.zero, move.relativeMove, animSpeed).y);
                    if (Mathf.Approximately(move.relativeMove.x + initPlayerPos.x, playerModels[move.entityID].transform.position.x) && Mathf.Approximately(move.relativeMove.y + initPlayerPos.y, playerModels[move.entityID].transform.position.z))
                    {
                        //moves the player and player model
                        GameStorage.playerMasterList[GameStorage.activePlayerList[move.entityID]].position.Set(Mathf.RoundToInt(GameStorage.playerMasterList[GameStorage.activePlayerList[move.entityID]].position.x + move.relativeMove.x), Mathf.RoundToInt(GameStorage.playerMasterList[GameStorage.activePlayerList[move.entityID]].position.y - move.relativeMove.y));
                        playerAnimMoves.Dequeue();

                        //if the player is done moving
                        if (playerAnimMoves.Count == 0)
                        {
                            updateTilesThisFrame = true;
                            battleState = BattleState.Attack;
                        }
                    }
                    break;
                case BattleState.Enemy:
                    selectedPlayer = -1;
                    if (enemyList[movingEnemy].cHealth > 0)
                    {
                        MoveEnemies();
                        battleState = BattleState.MoveEnemy;
                    }
                    else
                    {
                        movingEnemy++;
                        if (movingEnemy >= enemyList.Length)
                            EndEnemyTurn();
                    }
                    break;
                case BattleState.MoveEnemy:
                    MoveQueue animation = enemyAnimMoves.Peek();
                    Vector2 initEnemyPos = new Vector2(enemyList[animation.entityID].position.x + topLeft.x, (mapSizeY - 1) - enemyList[animation.entityID].position.y + topLeft.y);
                    enemyModels[animation.entityID].transform.Translate(Vector2.Lerp(Vector2.zero, animation.relativeMove, animSpeed).x, 0, Vector2.Lerp(Vector2.zero, animation.relativeMove, animSpeed).y);
                    if (Mathf.Approximately(animation.relativeMove.x + initEnemyPos.x, enemyModels[animation.entityID].transform.position.x) && Mathf.Approximately(animation.relativeMove.y + initEnemyPos.y, enemyModels[animation.entityID].transform.position.z))
                    {
                        //Moves the enemy and enemy model
                        enemyList[animation.entityID].position.Set(Mathf.RoundToInt(enemyList[animation.entityID].position.x + animation.relativeMove.x), Mathf.RoundToInt(enemyList[animation.entityID].position.y - animation.relativeMove.y));
                        enemyAnimMoves.Dequeue();
                        //If the enemy is done moving
                        if (enemyAnimMoves.Count == 0)
                        {
                            updateTilesThisFrame = true;
                            //Attacks if enemy can
                            if (chosenMove.attackPosition.x != -1)
                            {
                                PerformEnemyAttack(movingEnemy, chosenMove.attackPosition.x, chosenMove.attackPosition.y);
                            }
                            if (battleState != BattleState.ReturnCamera)
                            {
                                updateTilesThisFrame = true;
                                movingEnemy++;
                                if (movingEnemy >= enemyList.Length)
                                    EndEnemyTurn();
                                else
                                    battleState = BattleState.Enemy;
                            }
                        }
                    }
                    break;
                case BattleState.Swap:
                case BattleState.Player:
                case BattleState.Attack:
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Debug.Log(Input.mousePosition.x + ", " + Screen.width  + ", " + Input.mousePosition.x / Screen.width + " " + (Screen.height - Input.mousePosition.y) + ", " + Screen.height + ", " + ( 1 - Input.mousePosition.y / Screen.height));
                        Ray ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0));
                        RaycastHit hit;
                        int layerMask = 1 << 8;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                        {
                            print("I'm looking at " + hit.transform.GetComponent<BattleTile>().arrayID);
                            SpaceInteraction(hit.transform.GetComponent<BattleTile>().arrayID);
                            updateTilesThisFrame = true;
                        }
                        else
                            print("I'm looking at nothing!");
                    }
                    if (selectedSpell != -1)
                    {
                        updateTilesThisFrame = true;
                    }
                    break;
            }

            //If anything happened that could have changed the state of one or more tiles
            if (updateTilesThisFrame)
            {
                UpdateTileMap();
                for (int x = 0; x < mapSizeX; x++)
                {
                    for (int y = 0; y < mapSizeY; y++)
                    {
                        tileList[x, y].GetComponent<BattleTile>().UpdateColors();
                    }
                }
            }
            updateTilesThisFrame = false;
        }
    }

    /// <summary>
    /// Ends the enemy's turn and sets up the player's turn
    /// </summary>
    private void EndEnemyTurn()
    {
        //resets to allow players to move and starts player's turn
        foreach (int pID in GameStorage.activePlayerList)
        {
            if (GameStorage.playerMasterList[pID].cHealth > 0 && !GameStorage.playerMasterList[pID].statusList.Contains("sleep"))
                GameStorage.playerMasterList[pID].moved = false;
            GameStorage.playerMasterList[pID].EndOfTurn();
            GameStorage.playerMasterList[pID].StartOfTurn();
            CheckEventTriggers(GameStorage.playerMasterList[pID], EffectTriggers.StartOfTurn);
        }
        movingEnemy = 0;
        turn++;
        foreach (Enemy e in enemyList)
        {
            e.EndOfTurn();
            e.StartOfTurn();
            CheckEventTriggers(e, EffectTriggers.StartOfTurn);
        }
        if (battleState != BattleState.ReturnCamera)
            battleState = BattleState.Player;
    }

    /// <summary>
    /// Resets all variables and clears all visibles at the start and end of each battle
    /// </summary>
    private void ExpungeAll()
    {
        battleState = BattleState.None;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Destroy(tileList[x, y]);
                tileList[x, y] = null;
            }
        }
        for (int x = 0; x < playerModels.Length; x++)
        {
            Destroy(playerModels[x]);
            playerModels[x] = null;
        }
        for (int x = 0; x < enemyModels.Length; x++)
        {
            Destroy(enemyModels[x]);
            enemyModels[x] = null;
        }
        Destroy(battleCamera);
        battleCamera = null;
        Destroy(moveMarker);
        moveMarker = null;
    }

    /// <summary>
    /// Generates all of the tiles at the beginning of the battle
    /// </summary>
    /// <param name="xPos">X position of the left-most tile on the board</param>
    /// <param name="yPos">Y position of the up-most tile on the board</param>
    private void GenerateTileMap(int xPos, int yPos)
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileList[x, y] = Instantiate(battleTile, new Vector3(xPos + x, 0.5f, yPos + y), Quaternion.Euler(0, 0, 0));
                tileList[x, y].GetComponent<BattleTile>().arrayID = new Vector2Int(x, (mapSizeY - 1) - y);
                tileList[x, y].GetComponent<BattleTile>().battle = this;
            }
        }
    }

    /// <summary>
    /// Checks to see if all of one team is dead and triggers OnBattleEnd if so
    /// </summary>
    public void CheckForDeath()
    {
        int deadCount = 0;
        for (int pID = 0; pID < GameStorage.activePlayerList.Count; pID++)
        {
            if (GameStorage.playerMasterList[GameStorage.activePlayerList[pID]].cHealth <= 0)
            {
                deadCount++;
                playerModels[pID].SetActive(false);
                GameStorage.playerMasterList[GameStorage.activePlayerList[pID]].position.Set(-200, -200);
            }
        }
        if (deadCount == GameStorage.activePlayerList.Count)
            OnBattleEnd(false);
        deadCount = 0;
        for (int e = 0; e < enemyList.Length; e++)
        {
            if (enemyList[e].cHealth <= 0)
            {
                deadCount++;
                enemyModels[e].SetActive(false);
                enemyList[e].position.Set(-200, -200);
            }
        }
        if (deadCount == enemyList.Length)
            OnBattleEnd(true);
    }

    /// <summary>
    /// Triggered when all of one team is dead
    /// On won: Sets up camera return animation, gives control back to the player and hands out winnings
    /// On loss: Breaks everything
    /// </summary>
    /// <param name="won">If the battle was won by the player or not</param>
    public void OnBattleEnd(bool won)
    {
        if (won)
        {
            foreach (int p in GameStorage.activePlayerList)
            {
                GameStorage.playerMasterList[p].GainExp(200);
            }
            Cursor.lockState = CursorLockMode.Locked;
            mapPlayer.SetActive(true);
            cameraFinalPos = mapPlayer.GetComponentInChildren<Camera>().transform.position;
            cameraFinalRot = mapPlayer.GetComponentInChildren<Camera>().transform.rotation;
            mapPlayer.GetComponentInChildren<Camera>().transform.SetPositionAndRotation(battleCamera.transform.position, battleCamera.transform.rotation);
            ExpungeAll();
            updateTilesThisFrame = false;
            battleState = BattleState.ReturnCamera;
        }
        else
        {
            battleState = BattleState.None;
            ExpungeAll();
        }
    }

    /// <summary>
    /// Toggles whether enemy ranges are shown or not
    /// </summary>
    public void ToggleDangerArea()
    {
        showDanger = !showDanger;
        updateTilesThisFrame = true;
    }

    /// <summary>
    /// Toggles whether the aEther visual representation is shown or not
    /// </summary>
    public void ToggleaEtherView()
    {
        showaEther = !showaEther;
        updateTilesThisFrame = true;
    }

    /// <summary>
    /// Toggles between swap and move at start of battle
    /// </summary>
    public void ToggleSwap()
    {
        if (canSwap)
        {
            if (battleState == BattleState.Swap)
            {
                battleState = BattleState.Player;
            }
            else
            {
                battleState = BattleState.Swap;
            }
            selectedMoveSpot = new Vector2Int(-1, -1);
            selectedEnemy = -1;
            selectedPlayer = -1;
            moveMarker.SetActive(false);
            updateTilesThisFrame = true;
        }
    }

    /// <summary>
    /// If player wants to end the turn before all ally pawns have been moved
    /// </summary>
    public void EndPlayerTurnEarly()
    {
        moveMarker.SetActive(false);
        canSwap = false;
        foreach (int pID in GameStorage.activePlayerList)
        {
            GameStorage.playerMasterList[pID].moved = true;
        }
        FinishedMovingPawn();
    }

    /// <summary>
    /// Deals with all of the possibilities of what the player could want to do when they click on a tile
    /// </summary>
    /// <param name="pos">What tile they clicked on</param>
    public void SpaceInteraction(Vector2Int pos)
    {
        //If the player should actually be able to interact with a tile
        if (battleState == BattleState.Player || battleState == BattleState.Attack)
        {
            updateTilesThisFrame = true;

            bool actionTaken = false;
            switch (battleState)
            {
                case BattleState.Swap:
                    if (selectedPlayer != -1)
                    {
                        int n = PlayerAtPos(pos.x, pos.y);
                        GameStorage.playerMasterList[GameStorage.activePlayerList[n]].position = GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position;
                        GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position = pos;
                        playerModels[n].transform.position = new Vector3(GameStorage.playerMasterList[GameStorage.activePlayerList[n]].position.x + topLeft.x, 1, (mapSizeY - 1) - GameStorage.playerMasterList[GameStorage.activePlayerList[n]].position.y + topLeft.y);
                        playerModels[selectedPlayer].transform.position = new Vector3(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x + topLeft.x, 1, (mapSizeY - 1) - GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y + topLeft.y);
                        selectedPlayer = -1;
                        actionTaken = true;
                    }
                    break;
                case BattleState.Player:
                    //if player is moving something
                    if (tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerMoveRange && !GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].moved)
                    {
                        selectedMoveSpot.Set(pos.x, pos.y);
                        moveMarker.transform.position = new Vector3(pos.x + topLeft.x, 1, (mapSizeY - 1) - pos.y + topLeft.y);
                        moveMarker.SetActive(true);

                        //update the line renderer
                        moveMarker.GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
                        moveYFirst = true;
                        Vector2Int p = new Vector2Int(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x - selectedMoveSpot.x, GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y - selectedMoveSpot.y);

                        Vector2 moveDifference = new Vector2(selectedMoveSpot.x - GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x, selectedMoveSpot.y - GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y);

                        for (int y = 0; y <= Mathf.Abs(moveDifference.y); y++)
                        {
                            if (!GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].ValidMoveTile(battleMap[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x, GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y + y * Mathf.RoundToInt(Mathf.Sign(moveDifference.y))]))
                                moveYFirst = false;
                            if (EnemyAtPos(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x, GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y + y * Mathf.RoundToInt(Mathf.Sign(moveDifference.y))) != -1)
                                moveYFirst = false;
                        }

                        for (int x = 0; x <= Mathf.Abs(moveDifference.x); x++)
                        {
                            if (!GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].ValidMoveTile(battleMap[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x + x * Mathf.RoundToInt(Mathf.Sign(moveDifference.x)), selectedMoveSpot.y]))
                                moveYFirst = false;
                            if (EnemyAtPos(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x + x * Mathf.RoundToInt(Mathf.Sign(moveDifference.x)), selectedMoveSpot.y) != -1)
                                moveYFirst = false;
                        }

                        Debug.Log("Move Y First check 1 " + moveYFirst);
                        if (selectedMoveSpot.x != GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x)
                        {
                            p.x *= 2;
                        }
                        if (selectedMoveSpot.y != GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y)
                        {
                            p.y *= 2;
                        }

                        Debug.Log(moveYFirst);
                        if (moveYFirst)
                        {
                            moveMarker.GetComponent<LineRenderer>().SetPosition(1, new Vector3(p.x, 0, 0));
                            moveMarker.GetComponent<LineRenderer>().SetPosition(2, new Vector3(p.x, 0, -p.y));
                        }
                        else
                        {
                            moveMarker.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, -p.y));
                            moveMarker.GetComponent<LineRenderer>().SetPosition(2, new Vector3(p.x, 0, -p.y));
                        }
                        actionTaken = true;
                    }
                    break;
            }
            //If player tries to cast a spell
            if (selectedSpell != -1)
            {
                if (BattleTile.skillLegitTarget)
                {
                    selectedEnemy = EnemyAtPos(pos.x, pos.y);
                    //Generates the choice menu
                    if (selectedMoveSpot.x != -1)
                        skillCastConfirmMenu.GetComponentInChildren<Text>().text = "You have a move selected. Move and cast?";
                    else
                        skillCastConfirmMenu.GetComponentInChildren<Text>().text = "Are you sure you want to cast there?";
                    spellCastPosition = new Vector2Int(pos.x, pos.y);
                    skillCastConfirmMenu.SetActive(true);
                }
                actionTaken = true;
            }
            if (!actionTaken && hoveredSpell == -1)
            {
                //selecting a different player
                if (battleState != BattleState.Attack)
                {
                    selectedPlayer = PlayerAtPos(pos.x, pos.y);
                    selectedMoveSpot = new Vector2Int(-1, -1);
                    moveMarker.SetActive(false);
                    //selectedSpell = -1;
                }

                //selecting an enemy
                selectedEnemy = EnemyAtPos(pos.x, pos.y);
                if (tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerAttackRange)
                {
                    if (selectedEnemy == -1)
                        selectedEnemy = enemyList.Length + PlayerAtPos(pos.x, pos.y);
                }
            }
        }
    }

    /// <summary>
    /// Sets up the movement animation to the position they want to go to for the player
    /// </summary>
    public void ConfirmPlayerMove()
    {
        if (selectedMoveSpot.x != -1)
        {
            if (moveYFirst)
            {
                if (!Mathf.Approximately(((mapSizeY - 1) - selectedMoveSpot.y) - playerModels[selectedPlayer].transform.position.z, 0))
                    playerAnimMoves.Enqueue(new MoveQueue(selectedPlayer, new Vector2(0, ((mapSizeY - 1) - selectedMoveSpot.y) - playerModels[selectedPlayer].transform.position.z + topLeft.y)));
                if (!Mathf.Approximately(selectedMoveSpot.x - playerModels[selectedPlayer].transform.position.x, 0))
                    playerAnimMoves.Enqueue(new MoveQueue(selectedPlayer, new Vector2(selectedMoveSpot.x - playerModels[selectedPlayer].transform.position.x + topLeft.x, 0)));
            }
            else
            {
                if (!Mathf.Approximately(selectedMoveSpot.x - playerModels[selectedPlayer].transform.position.x, 0))
                    playerAnimMoves.Enqueue(new MoveQueue(selectedPlayer, new Vector2(selectedMoveSpot.x - playerModels[selectedPlayer].transform.position.x + topLeft.x, 0)));
                if (!Mathf.Approximately(((mapSizeY - 1) - selectedMoveSpot.y) - playerModels[selectedPlayer].transform.position.z, 0))
                    playerAnimMoves.Enqueue(new MoveQueue(selectedPlayer, new Vector2(0, ((mapSizeY - 1) - selectedMoveSpot.y) - playerModels[selectedPlayer].transform.position.z + topLeft.y)));
            }
            selectedMoveSpot = new Vector2Int(-1, -1);
            moveMarker.SetActive(false);
            canSwap = false;
            battleState = BattleState.MovePlayer;
        }
    }

    /// <summary>
    /// Finds the optimal move for the enemy currently moving
    /// </summary>
    private void MoveEnemies()
    {
        /*
         * Difficulty of the battles determines what variables will be taken into account (higher difficulties will also take into account lower difficulty variables):
         * 1: super easy - just moves towards and attacks nearest player
         * 2: easy - takes into account how much damage they can do, approx damage they'll take in return, and ability to be counterattacked
         * 3: medium - checks who their final position would put them near, both enemy and ally (raw number version), whether they will be near an ally healer, and whether the opponent target is a healer
         * 4: hard - starts knowing attacking can be the wrong move, takes into account blocking for more important allies(healers, ranged carries)
         * 5: why? - checks who their final position would put them near, both enemy and ally (including damage each enemy can do to them, return damage if possible)
         * 
         * Each enemy also has an aggression and a ppa(pack play value) value. These values determine the weight of the previous checks:
         * 
         * Aggression determines their likelyhood to take fights and whether they are scared of being outnumbered
         * 1-3 = cowardly: very unlikely to take fights stacked against them, prefering to hang back and pick off low health enemies
         * 4-7 = balanced: open to any situation, has qualities of both sides
         * 8-10 = aggressive: likely to challenge any enemy no matter their heath or backup
         * 
         * PPA determines how likely they are to play with the rest of their team
         * 1-3 = lone wolf: prefers to fight on their own
         * 4-7 = meh: could care less
         * 8-10 = social animal: finds strength in numbers, usually playing around and protecting others
        */

        int n = movingEnemy;
        List<EnemyMove> possibleMoves = new List<EnemyMove>();
        EnemyMove fallbackMove = new EnemyMove(0, 0, 100, 0, true);
        List<Pair<Vector2Int, bool>> moveSpots = GetViableMovements(enemyList[n]);
        WeaponType weapon;
        if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[enemyList[n].equippedWeapon.Name]).subType, out weapon))
            Debug.Log("Weapon Type does not exist in the Registry.");
        foreach (Pair<Vector2Int, bool> pos in moveSpots)
        {
            List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, new Vector2Int(pos.First.x, pos.First.y));
            bool attacksThisMove = false;
            foreach (Vector2Int aPos in attackSpots)
            {
                if (PlayerAtPos(aPos.x, aPos.y) != -1)
                {
                    possibleMoves.Add(new EnemyMove(pos.First.x, pos.First.y, aPos.x, aPos.y, 15 - (Mathf.Abs(pos.First.x - enemyList[n].position.x) + Mathf.Abs(pos.First.y - enemyList[n].position.y)), 1, pos.Second));
                    attacksThisMove = true;
                }
            }
            if (!attacksThisMove)
            {
                foreach (int pID in GameStorage.activePlayerList)
                {
                    //If this move is the closest the enemy can get to a player, make it the move that happens if no attacks are possible
                    if (Mathf.Abs(GameStorage.playerMasterList[pID].position.x - pos.First.x) + Mathf.Abs(GameStorage.playerMasterList[pID].position.y - pos.First.y) < fallbackMove.priority)
                    {
                        fallbackMove = new EnemyMove(pos.First.x, pos.First.y, Mathf.Abs(GameStorage.playerMasterList[pID].position.x - pos.First.x) + Mathf.Abs(GameStorage.playerMasterList[pID].position.y - pos.First.y), 0, pos.Second);
                    }
                    //If this move ties with the current fallback move for distance from a player, pick a random one
                    else if (Mathf.Abs(GameStorage.playerMasterList[pID].position.x - pos.First.x) + Mathf.Abs(GameStorage.playerMasterList[pID].position.y - pos.First.y) == fallbackMove.priority)
                    {
                        if (Random.Range(0, 2) == 1)
                        {
                            fallbackMove = new EnemyMove(pos.First.x, pos.First.y, Mathf.Abs(GameStorage.playerMasterList[pID].position.x - pos.First.x) + Mathf.Abs(GameStorage.playerMasterList[pID].position.y - pos.First.y), 0, pos.Second);
                        }
                    }
                }
            }
        }
        //if the enemy can't attack anyone, adds the move that would get them closest to the nearest player
        if (possibleMoves.Count == 0)
        {
            possibleMoves.Add(fallbackMove);
        }
        //sorts the possible moves in order of priority
        possibleMoves.Sort(delegate (EnemyMove c1, EnemyMove c2) { return c1.CompareTo(c2); });

        //chooses between moves with equal priority
        while (possibleMoves.Count > 1 && possibleMoves[0].CompareTo(possibleMoves[1]) == 0)
        {
            possibleMoves.RemoveAt(Random.Range(0, 2));
        }
        Debug.Log(possibleMoves[0].yFirst);
        if (possibleMoves[0].yFirst)
        {
            if (!Mathf.Approximately(((mapSizeY - 1) - possibleMoves[0].movePosition.y) - enemyModels[n].transform.position.z, 0))
                enemyAnimMoves.Enqueue(new MoveQueue(n, new Vector2(0, ((mapSizeY - 1) - possibleMoves[0].movePosition.y) - enemyModels[n].transform.position.z + topLeft.y)));
            if (!Mathf.Approximately(possibleMoves[0].movePosition.x - enemyModels[n].transform.position.x, 0))
                enemyAnimMoves.Enqueue(new MoveQueue(n, new Vector2(possibleMoves[0].movePosition.x - enemyModels[n].transform.position.x + topLeft.x, 0)));
        }
        else
        {
            if (!Mathf.Approximately(possibleMoves[0].movePosition.x - enemyModels[n].transform.position.x, 0))
                enemyAnimMoves.Enqueue(new MoveQueue(n, new Vector2(possibleMoves[0].movePosition.x - enemyModels[n].transform.position.x + topLeft.x, 0)));
            if (!Mathf.Approximately(((mapSizeY - 1) - possibleMoves[0].movePosition.y) - enemyModels[n].transform.position.z, 0))
                enemyAnimMoves.Enqueue(new MoveQueue(n, new Vector2(0, ((mapSizeY - 1) - possibleMoves[0].movePosition.y) - enemyModels[n].transform.position.z + topLeft.y)));
        }

        enemyList[n].moved = true;
        chosenMove = possibleMoves[0];
    }

    /// <summary>
    /// Gets how much damage p1 would do when attacking p2
    /// </summary>
    /// <param name="p1">The pawn doing the attacking</param>
    /// <param name="p2">The pawn getting attacked</param>
    /// <returns></returns>
    public int GetDamageValues(BattleParticipant p1, BattleParticipant p2)
    {
        //gets the distance between the player and enemy
        int dist = Mathf.Abs(p2.position.x - p1.position.x);
        if (Mathf.Abs(p2.position.y - p1.position.y) > dist)
            dist = Mathf.Abs(p2.position.y - p1.position.y);

        int type = ((EquippableBase)Registry.ItemRegistry[p1.equippedWeapon.Name]).statType;
        foreach (RangeDependentAttack r in Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[p1.equippedWeapon.Name]).subType].specialRanges)
        {
            if (r.atDistance == dist)
            {
                type = r.damageType;
            }
        }
        if (type == 0)
            return Mathf.RoundToInt((p1.GetEffectiveAtk(dist) * 3.0f) / p2.GetEffectiveDef(dist));
        else
            return Mathf.RoundToInt((p1.GetEffectiveMAtk(dist) * 3.0f) / p2.GetEffectiveMDef(dist));
    }

    /// <summary>
    /// Performs attack, then checks for an executes possible counterattack if both pawns are still alive
    /// </summary>
    public void PerformPlayerAttack()
    {
        WeaponType pweapon;
        if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].equippedWeapon.Name]).subType, out pweapon))
            Debug.Log("Weapon Type does not exist in the Registry.");
        //if healing a player
        if (selectedEnemy >= enemyList.Length)
        {
            GameStorage.playerMasterList[GameStorage.activePlayerList[selectedEnemy - enemyList.Length]].Heal(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].GetEffectiveAtk() / 2);
        }
        //if attacking an enemy
        else
        {
            float mod = 1.0f;
            if (Random.Range(0.0f, 100.0f) < GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].critChance + ((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].equippedWeapon.Name]).critChanceMod) { mod = 1.5f; }
            enemyList[selectedEnemy].Damage(Mathf.RoundToInt(GetDamageValues(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]], enemyList[selectedEnemy]) * mod));
            if (enemyList[selectedEnemy].cHealth <= 0)
            {
                CheckForDeath();
            }
            else
            {
                WeaponType eweapon;
                if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[enemyList[selectedEnemy].equippedWeapon.Name]).subType, out eweapon))
                    Debug.Log("Weapon Type does not exist in the Registry.");
                if (pweapon.ranged == eweapon.ranged)
                {
                    mod = 1.0f;
                    if (Random.Range(0.0f, 100.0f) < enemyList[selectedEnemy].critChance + ((EquippableBase)Registry.ItemRegistry[enemyList[selectedEnemy].equippedWeapon.Name]).critChanceMod) { mod = 1.5f; }
                    GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].Damage(Mathf.RoundToInt(GetDamageValues(enemyList[selectedEnemy], GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]]) * mod));
                    if (GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].cHealth <= 0)
                    {
                        playerModels[selectedPlayer].SetActive(false);
                        GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.Set(-200, -200);
                        CheckForDeath();
                    }
                }
            }
        }
        EndPlayerAttack();
    }

    /// <summary>
    /// Performs attack, then checks for an executes possible counterattack if both pawns are still alive
    /// </summary>
    /// <param name="enemy">The ID of the attacking enemy</param>
    /// <param name="pX">X coordinate of the tile Where the player being attacked is</param>
    /// <param name="pY">Y coordinate of the tile where the player being attacked is</param>
    public void PerformEnemyAttack(int enemy, int pX, int pY)
    {
        int player = PlayerAtPos(pX, pY);

        float mod = 1.0f;
        if (Random.Range(0.0f, 100.0f) < enemyList[enemy].critChance + ((EquippableBase)Registry.ItemRegistry[enemyList[enemy].equippedWeapon.Name]).critChanceMod) { mod = 1.5f; }
        GameStorage.playerMasterList[GameStorage.activePlayerList[player]].Damage(Mathf.RoundToInt(GetDamageValues(enemyList[enemy], GameStorage.playerMasterList[GameStorage.activePlayerList[player]]) * mod));
        if (GameStorage.playerMasterList[GameStorage.activePlayerList[player]].cHealth <= 0)
        {
            playerModels[player].SetActive(false);
            GameStorage.playerMasterList[GameStorage.activePlayerList[player]].position.Set(-200, -200);
            CheckForDeath();
        }
        else
        {
            WeaponType pweapon;
            WeaponType eweapon;
            if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[enemyList[enemy].equippedWeapon.Name]).subType, out eweapon))
                Debug.Log("Weapon Type does not exist in the Registry.");
            if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[player]].equippedWeapon.Name]).subType, out pweapon))
                Debug.Log("Weapon Type does not exist in the Registry.");
            if (pweapon.ranged == eweapon.ranged)
            {
                mod = 1.0f;
                if (Random.Range(0.0f, 100.0f) < GameStorage.playerMasterList[GameStorage.activePlayerList[player]].critChance + ((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[player]].equippedWeapon.Name]).critChanceMod) { mod = 1.5f; }
                enemyList[enemy].Damage(Mathf.RoundToInt(GetDamageValues(GameStorage.playerMasterList[GameStorage.activePlayerList[player]], enemyList[enemy]) * mod));
                if (enemyList[enemy].cHealth <= 0)
                {
                    CheckForDeath();
                    Debug.Log(battleState);
                }
            }
        }
    }

    /// <summary>
    /// If the player decides not to do anything after moving
    /// </summary>
    public void EndPlayerAttack()
    {
        GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].moved = true;
        FinishedMovingPawn();
    }

    /// <summary>
    /// Sets up for the next pawn to be moved
    /// or
    /// If all player pawns have finished moving, set up for enemies to move
    /// </summary>
    public void FinishedMovingPawn()
    {
        if (battleState != BattleState.ReturnCamera)
        {
            battleState = BattleState.Player;
            selectedMoveSpot = new Vector2Int(-1, -1);
            selectedPlayer = -1;
            selectedEnemy = -1;
            selectedSpell = -1;
            updateTilesThisFrame = true;

            //checks if all players are done moving
            bool playersDone = true;
            foreach (int pID in GameStorage.activePlayerList)
            {
                if (!GameStorage.playerMasterList[pID].moved)
                    playersDone = false;
            }
            if (playersDone)
            {
                //resets to start enemy moves
                for (int j = 0; j < enemyList.Length; j++)
                {
                    if (enemyList[j].cHealth > 0)
                        enemyList[j].moved = false;
                }
                battleState = BattleState.Enemy;
            }
        }
    }

    /// <summary>
    /// Checks if there is an player at the given x and y values
    /// </summary>
    private int PlayerAtPos(int x, int y)
    {
        foreach (int pID in GameStorage.activePlayerList)
        {
            if (GameStorage.playerMasterList[pID].position.x == x && GameStorage.playerMasterList[pID].position.y == y)
                return pID;
        }
        return -1;
    }

    /// <summary>
    /// Checks if there is an enemy at the given x and y values
    /// </summary>
    private int EnemyAtPos(int x, int y)
    {
        for (int e = 0; e < enemyList.Length; e++)
        {
            if (enemyList[e].position.x == x && enemyList[e].position.y == y)
                return e;
        }
        return -1;
    }

    /// <summary>
    /// Updates the data of each tile in the battlefield with its significance at the current moment
    /// </summary>
    private void UpdateTileMap()
    {
        //resets the tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileList[x, y].GetComponent<BattleTile>().Reset();

                //updates the aEther viewer
                tileList[x, y].GetComponentsInChildren<Renderer>()[1].enabled = showaEther;
                if (showaEther)
                    tileList[x, y].GetComponentsInChildren<Transform>()[1].localScale = new Vector3(0.1f * aEtherMap[x, y, 0], 0.01f, 0.1f * aEtherMap[x, y, 0]);
            }
        }

        //shows skill range and what is targettable within that range if a spell is selected or hovered
        int skillToShow = selectedSpell;
        if (hoveredSpell != -1)
            skillToShow = hoveredSpell;
        if (skillToShow != -1)
        {
            Vector2Int skillPos = GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position;
            if (selectedMoveSpot.x != -1)
                skillPos = selectedMoveSpot;
            Skill displaySkill = GameStorage.skillTreeList[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].skillQuickList[skillToShow - 1].x][GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].skillQuickList[skillToShow - 1].y];

            if (displaySkill.targetType == TargettingType.Self)
            {
                tileList[skillPos.x, (mapSizeY - 1) - skillPos.y].GetComponent<BattleTile>().skillTargettable = true;
            }
            else
            {
                for (int x = -displaySkill.targettingRange; x <= displaySkill.targettingRange; x++)
                {
                    for (int y = -displaySkill.targettingRange; y <= displaySkill.targettingRange; y++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= displaySkill.targettingRange && x + skillPos.x >= 0 && x + skillPos.x < mapSizeX && y + skillPos.y >= 0 && y + skillPos.y < mapSizeY)
                        {
                            if (displaySkill.targetType == TargettingType.All)
                            {
                                tileList[x + skillPos.x, (mapSizeY - 1) - (y + skillPos.y)].GetComponent<BattleTile>().skillTargettable = true;
                            }
                            else
                            {
                                tileList[x + skillPos.x, (mapSizeY - 1) - (y + skillPos.y)].GetComponent<BattleTile>().skillRange = true;
                            }
                            if (displaySkill.targetType == TargettingType.Ally && PlayerAtPos(x + skillPos.x, y + skillPos.y) != -1)
                            {
                                tileList[x + skillPos.x, (mapSizeY - 1) - (y + skillPos.y)].GetComponent<BattleTile>().skillTargettable = true;
                            }
                            else if (displaySkill.targetType == TargettingType.Enemy && EnemyAtPos(x + skillPos.x, y + skillPos.y) != -1)
                            {
                                tileList[x + skillPos.x, (mapSizeY - 1) - (y + skillPos.y)].GetComponent<BattleTile>().skillTargettable = true;
                            }
                        }
                    }
                }
            }
            if (selectedSpell != -1)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0));
                RaycastHit hit;
                int layerMask = 1 << 8;
                if (Physics.Raycast(ray, out hit, 30.0f, layerMask))
                {
                    int iX = hit.transform.GetComponent<BattleTile>().arrayID.x;
                    int iY = hit.transform.GetComponent<BattleTile>().arrayID.y;
                    if (tileList[iX, (mapSizeY - 1) - iY].GetComponent<BattleTile>().skillTargettable)
                        BattleTile.skillLegitTarget = true;
                    else
                        BattleTile.skillLegitTarget = false;
                    for (int x = -Mathf.FloorToInt((displaySkill.xRange - 1) / 2.0f); x <= Mathf.CeilToInt((displaySkill.xRange - 1) / 2.0f); x++)
                    {
                        for (int y = -Mathf.FloorToInt((displaySkill.yRange - 1) / 2.0f); y <= Mathf.CeilToInt((displaySkill.yRange - 1) / 2.0f); y++)
                        {
                            if (x + iX >= 0 && x + iX < mapSizeX && y + iY >= 0 && y + iY < mapSizeY)
                            {
                                tileList[x + iX, (mapSizeY - 1) - (y + iY)].GetComponent<BattleTile>().skillTargetting = true;
                            }
                        }
                    }
                }
            }
        }
        if (battleState == BattleState.Swap)
        {
            for (int p = 0; p < GameStorage.activePlayerList.Count; p++)
            {
                tileList[GameStorage.playerMasterList[GameStorage.activePlayerList[p]].position.x, (mapSizeY - 1) - GameStorage.playerMasterList[GameStorage.activePlayerList[p]].position.y].GetComponent<BattleTile>().playerMoveRange = true;
            }
            return;
        }

        //if we need to render player moves
        else if (battleState == BattleState.Player && selectedPlayer != -1)
        {
            List<Pair<Vector2Int, bool>> moveSpots = GetViableMovements(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]]);
            WeaponType weapon;
            if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].equippedWeapon.Name]).subType, out weapon))
                Debug.Log("Weapon Type does not exist in the Registry.");
            foreach (Pair<Vector2Int, bool> pos in moveSpots)
            {
                tileList[pos.First.x, (mapSizeY - 1) - pos.First.y].GetComponent<BattleTile>().playerMoveRange = true;
                RenderWeaponRanges(pos.First.x, pos.First.y, weapon, "attack area");
            }
        }
        if (showDanger)
        {
            for (int n = 0; n < enemyList.Length; n++)
            {
                List<Pair<Vector2Int, bool>> moveSpots = GetViableMovements(enemyList[n]);
                WeaponType weapon;
                if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[enemyList[n].equippedWeapon.Name]).subType, out weapon))
                    Debug.Log("Weapon Type does not exist in the Registry.");
                foreach (Pair<Vector2Int, bool> pos in moveSpots)
                {
                    tileList[pos.First.x, (mapSizeY - 1) - pos.First.y].GetComponent<BattleTile>().enemyDanger = true;
                    RenderWeaponRanges(pos.First.x, pos.First.y, weapon, "danger area");
                }
            }
        }
        if (battleState == BattleState.Attack)
        {
            WeaponType weapon;
            if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].equippedWeapon.Name]).subType, out weapon))
                Debug.Log("Weapon Type does not exist in the Registry.");
            RenderWeaponRanges(GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.x, GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].position.y, weapon, "attack area");
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    if ((EnemyAtPos(x, y) == -1 || ((EquippableBase)Registry.ItemRegistry[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].equippedWeapon.Name]).strength == 0) && (PlayerAtPos(x, y) == -1 || !weapon.heals))
                        tileList[x, (mapSizeY - 1) - y].GetComponent<BattleTile>().playerAttackRange = false;
                }
            }
        }
    }

    /// <summary>
    /// Changes the tiles around the specified position to show the weapon range from that point
    /// </summary>
    /// <param name="x">The grid x position of the spot to check around</param>
    /// <param name="y">The grid y position of the spot to check around</param>
    /// <param name="weapon">What weapon type is being checked. Contains the range and if it is ranged or not</param>
    /// <param name="tileValue">Whether this check is for an enemy (danger area) or a player (attack area)</param>
    public void RenderWeaponRanges(int x, int y, WeaponType weapon, string tileValue)
    {
        List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, new Vector2Int(x, y));
        foreach (Vector2Int pos in attackSpots)
        {
            tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().ChangeValueByKey(tileValue);
        }
    }

    /// <summary>
    /// Gets all of the places a given pawn can move to
    /// </summary>
    /// <param name="entity">The entity moving</param>
    /// <returns>First = a valid position, Second = whether or not moving vertically first is valid</returns>
    private List<Pair<Vector2Int, bool>> GetViableMovements(BattleParticipant entity)
    {
        List<Pair<Vector2Int, bool>> moveSpots = new List<Pair<Vector2Int, bool>>();
        bool isPlayer = entity is Player;
        int maxMove = entity.GetMoveSpeed();
        WeaponType weapon;
        if (!Registry.WeaponTypeRegistry.TryGetValue(((EquippableBase)Registry.ItemRegistry[entity.equippedWeapon.Name]).subType, out weapon))
            Debug.Log("Weapon Type does not exist in the Registry.");
        for (int x = -maxMove; x <= maxMove; x++)
        {
            for (int y = -maxMove; y <= maxMove; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= maxMove && x + entity.position.x >= 0 && x + entity.position.x < mapSizeX && y + entity.position.y >= 0 && y + entity.position.y < mapSizeY)
                {
                    //It is automatically valid if the entity is moving to itself
                    if (x == 0 && y == 0)
                    {
                        moveSpots.Add(new Pair<Vector2Int, bool>(new Vector2Int(entity.position.x, entity.position.y), true));
                        continue;
                    }
                    bool goodTile = true;
                    //It is an invalid move position if it would overlap with an existing entity
                    if (PlayerAtPos(x + entity.position.x, y + entity.position.y) != -1)
                        goodTile = false;
                    if (EnemyAtPos(x + entity.position.x, y + entity.position.y) != -1)
                        goodTile = false;
                    //If it passes all of the preliminary tests
                    if (goodTile)
                    {
                        //Check every tile along the path, starting by moving vertically
                        for (int cY = 1; cY <= Mathf.Abs(y); cY++)
                        {
                            if (!entity.ValidMoveTile(battleMap[entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y]))
                            {
                                goodTile = false;
                            }
                            //Checks if a member of the opposing team is in the way
                            if (isPlayer ? EnemyAtPos(entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y) != -1 : PlayerAtPos(entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y) != -1)
                                goodTile = false;
                        }
                        //If all tiles along the vertical path pass the test, check horizontal from the end of that path
                        if (goodTile)
                        {
                            for (int cX = 1; cX <= Mathf.Abs(x); cX++)
                            {
                                if (!entity.ValidMoveTile(battleMap[cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, y + entity.position.y]))
                                {
                                    goodTile = false;
                                }
                                //Checks if a member of the opposing team is in the way
                                if (isPlayer ? EnemyAtPos(cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, y + entity.position.y) != -1 : PlayerAtPos(cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, y + entity.position.y) != -1)
                                    goodTile = false;
                            }
                            //If it passes the y-first set of tests, mark it as a valid move tile by moving y first
                            if (goodTile)
                            {
                                moveSpots.Add(new Pair<Vector2Int, bool>(new Vector2Int(x + entity.position.x, y + entity.position.y), true));
                                continue;
                            }
                        }
                        //if invalid by y, x, check x, y
                        goodTile = true;
                        //Check every tile along the path, starting by moving horizontally
                        for (int cX = 1; cX <= Mathf.Abs(x); cX++)
                        {
                            if (!entity.ValidMoveTile(battleMap[cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, entity.position.y]))
                            {
                                goodTile = false;
                            }
                            //Checks if a member of the opposing team is in the way
                            if (isPlayer ? EnemyAtPos(cX * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.x, entity.position.y) != -1 : PlayerAtPos(cX * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.x, entity.position.y) != -1)
                                goodTile = false;
                        }
                        //If all tiles along the horizontal path pass the test, check vertical from the end of that path
                        if (goodTile)
                        {
                            for (int cY = 1; cY <= Mathf.Abs(y); cY++)
                            {
                                if (!entity.ValidMoveTile(battleMap[x + entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y]))
                                {
                                    goodTile = false;
                                }
                                //Checks if a member of the opposing team is in the way
                                if (isPlayer ? EnemyAtPos(x + entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y) != -1 : PlayerAtPos(x + entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y) != -1)
                                    goodTile = false;
                            }
                            //If it passes the x-first set of tests, mark it as a valid move tile by moving x first
                            if (goodTile)
                            {
                                moveSpots.Add(new Pair<Vector2Int, bool>(new Vector2Int(x + entity.position.x, y + entity.position.y), false));
                            }
                        }
                    }
                }
            }
        }
        return moveSpots;
    }

    /// <summary>
    /// Returns a list of all the spaces attackable from a given position by the given weapon type
    /// </summary>
    private List<Vector2Int> GetViableAttackSpaces(WeaponType weapon, Vector2Int center)
    {
        List<Vector2Int> attackSpots = new List<Vector2Int>();
        for (int i = 1; i <= weapon.range; i++)
        {
            //If the weapon is melee and the space is not directly next to the attacker there needs to be a clear line to it
            if (!weapon.ranged && i > 1)
            {
                if (center.x + weapon.sRange + i < mapSizeX && battleMap[center.x + weapon.sRange + i, (mapSizeY - 1) - center.y] != 6 && attackSpots.Contains(new Vector2Int(center.x + weapon.sRange + i - 1, center.y)))
                    attackSpots.Add(new Vector2Int(center.x + weapon.sRange + i, center.y));

                if (center.x - weapon.sRange - i >= 0 && battleMap[center.x - weapon.sRange - i, (mapSizeY - 1) - center.y] != 6 && attackSpots.Contains(new Vector2Int(center.x - weapon.sRange - i + 1, center.y)))
                    attackSpots.Add(new Vector2Int(center.x - weapon.sRange - i, center.y));

                if (center.y + weapon.sRange + i < mapSizeY && battleMap[center.x, (mapSizeY - 1) - (center.y + weapon.sRange + i)] != 6 && attackSpots.Contains(new Vector2Int(center.x, center.y + weapon.sRange + i - 1)))
                    attackSpots.Add(new Vector2Int(center.x, center.y + weapon.sRange + i));

                if (center.y - weapon.sRange - i >= 0 && battleMap[center.x, (mapSizeY - 1) - (center.y - weapon.sRange - i)] != 6 && attackSpots.Contains(new Vector2Int(center.x, center.y - weapon.sRange - i + 1)))
                    attackSpots.Add(new Vector2Int(center.x, center.y - weapon.sRange - i));
            }
            //If the weapon is ranged or the space is right against the attacker it just needs to be a place entities can exist
            else
            {
                if (center.x + weapon.sRange + i < mapSizeX && battleMap[center.x + weapon.sRange + i, (mapSizeY - 1) - center.y] != 6)
                    attackSpots.Add(new Vector2Int(center.x + weapon.sRange + i, center.y));
                if (center.x - weapon.sRange - i >= 0 && battleMap[center.x - weapon.sRange - i, (mapSizeY - 1) - center.y] != 6)
                    attackSpots.Add(new Vector2Int(center.x - weapon.sRange - i, center.y));
                if (center.y + weapon.sRange + i < mapSizeY && battleMap[center.x, (mapSizeY - 1) - (center.y + weapon.sRange + i)] != 6)
                    attackSpots.Add(new Vector2Int(center.x, center.y + weapon.sRange + i));
                if (center.y - weapon.sRange - i >= 0 && battleMap[center.x, (mapSizeY - 1) - (center.y - weapon.sRange - i)] != 6)
                    attackSpots.Add(new Vector2Int(center.x, center.y - weapon.sRange - i));
            }
        }
        //Checks the diagonal attack spaces if there are any
        for (int i = 1; i <= weapon.diagCut; i++)
        {
            if (center.y + i < mapSizeY)
            {
                if (center.x - i >= 0 && battleMap[center.x - i, (mapSizeY - 1) - (center.y + 1)] != 6)
                {
                    attackSpots.Add(new Vector2Int(center.x - i, center.y + i));
                }
                if (center.x + i < mapSizeX && battleMap[center.x + i, (mapSizeY - 1) - (center.y + 1)] != 6)
                {
                    attackSpots.Add(new Vector2Int(center.x + i, center.y + i));
                }
            }
            if (center.y - i >= 0)
            {
                if (center.x - i >= 0 && battleMap[center.x - i, (mapSizeY - 1) - (center.y - 1)] != 6)
                {
                    attackSpots.Add(new Vector2Int(center.x - i, center.y - i));
                }
                if (center.x + i < mapSizeX && battleMap[center.x + i, (mapSizeY - 1) - (center.y - 1)] != 6)
                {
                    attackSpots.Add(new Vector2Int(center.x + i, center.y - i));
                }
            }
        }
        return attackSpots;
    }

    /// <summary>
    /// Checks for and executes and 
    /// </summary>
    /// <param name="triggered">The pawn we are checking</param>
    /// <param name="trigger">The type of trigger that was tripped</param>
    /// <param name="other">If the event involved another pawn, such as taking damage or giving healing</param>
    /// <param name="data">If the event has other important data, such as the amount of damage taken</param>
    private void CheckEventTriggers(BattleParticipant triggered, EffectTriggers trigger, BattleParticipant other = null, int data = 0)
    {
        List<SkillPartBase> list = triggered.GetTriggeredEffects(trigger);
        Debug.Log(list.Count);
        foreach(SkillPartBase effect in list)
        {
            if (effect.targetType == TargettingType.Self)
            {
                CastSkillEffect(effect, triggered, triggered);
            }
            else
            {
                CastSkillEffect(effect, triggered, other);
            }
        }
    }

    #region Spell Casting

    /// <summary>
    /// Activates when a spell button starts being hovered
    /// </summary>
    public void HoveringSpell(int buttonID)
    {
        hoveredSpell = buttonID;
        updateTilesThisFrame = true;
    }

    /// <summary>
    /// Activates when a spell button is no longer hovered
    /// </summary>
    public void StopHoveringSpell()
    {
        hoveredSpell = -1;
        updateTilesThisFrame = true;
    }

    /// <summary>
    /// Called then the player selects a spell they want to try and cast from their quick cast list
    /// </summary>
    /// <param name="buttonID">The place in the spell quick list to grab the spell from</param>
    public void SelectSpell(int buttonID)
    {
        if (selectedSpell == buttonID)
            selectedSpell = -1;
        else
            selectedSpell = buttonID;
        updateTilesThisFrame = true;
    }

    /// <summary>
    /// Enacts all of a spell's effects at a single coordinate space
    /// </summary>
    /// <param name="castedSpell">What spell is being cast</param>
    /// <param name="castedX">The x coordinate of the space to affect</param>
    /// <param name="castedY">The y coordinate of the space to affect</param>
    /// <param name="IDUsingMove">If it is greater than the amount for players, it denotes an enemy</param>
    public void CastSkill(Skill castedSpell, int castedX, int castedY, BattleParticipant caster)
    {
        Debug.Log(castedX + " " + castedY);
        foreach (SkillPartBase effect in castedSpell.partList)
        {
            if (effect.targetType == TargettingType.Self)
            {
                CastSkillEffect(effect, caster, caster);
            }
            else
            {
                bool castedByPlayer = caster is Player;
                for (int x = -Mathf.FloorToInt((castedSpell.xRange - 1) / 2.0f); x <= Mathf.CeilToInt((castedSpell.xRange - 1) / 2.0f); x++)
                {
                    for (int y = -Mathf.FloorToInt((castedSpell.yRange - 1) / 2.0f); y <= Mathf.CeilToInt((castedSpell.yRange - 1) / 2.0f); y++)
                    {
                        if (effect.targetType == TargettingType.Ally || effect.targetType == TargettingType.All)
                        {
                            //If there is someone from the same team at the position
                            if (castedByPlayer)
                            {
                                if (PlayerAtPos(x + castedX, y + castedY) != -1)
                                {
                                    Debug.Log("Trying to make it to casting the effect1:" + x + "|" + y);
                                    CastSkillEffect(effect, caster, GameStorage.playerMasterList[GameStorage.activePlayerList[PlayerAtPos(x + castedX, y + castedY)]]);
                                }
                            }
                            else if (EnemyAtPos(x + castedX, y + castedY) != -1)
                                CastSkillEffect(effect, caster, enemyList[EnemyAtPos(x + castedX, y + castedY)]);
                        }
                        if(effect.targetType == TargettingType.Enemy || effect.targetType == TargettingType.All)
                        {
                            //If there is someone from the opposing team at the position
                            if (castedByPlayer)
                            {
                                if (EnemyAtPos(x + castedX, y + castedY) != -1)
                                {
                                    Debug.Log("Trying to make it to casting the effect2:" + x + "|" + y + " " + EnemyAtPos(x + castedX, y + castedY));
                                    CastSkillEffect(effect, caster, enemyList[EnemyAtPos(x + castedX, y + castedY)]);
                                }
                            }
                            else if (PlayerAtPos(x + castedX, y + castedY) != -1)
                                CastSkillEffect(effect, caster, GameStorage.playerMasterList[GameStorage.activePlayerList[PlayerAtPos(x + castedX, y + castedY)]]);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Executes the effects of a skill part on a pawn
    /// </summary>
    /// <param name="effect">The skill part to execute</param>
    /// <param name="caster">The caster of the skill</param>
    /// <param name="target">The target for the skill</param>
    public void CastSkillEffect(SkillPartBase effect, BattleParticipant caster, BattleParticipant target)
    {
        Debug.Log("Made it to casting the effect");

        //Flat damage, then calculated damage, then remaining hp, then max hp
        if (effect is DamagePart)
        {
            DamagePart trueEffect = effect as DamagePart;
            target.Damage(trueEffect.flatDamage);
            target.Damage(Mathf.RoundToInt((trueEffect.damage * caster.mAttack * 3.0f) / target.GetEffectiveDef()));
            target.Damage((int)(target.cHealth * trueEffect.remainingHpPercent / 100.0f));
            target.Damage((int)(target.mHealth * trueEffect.maxHpPercent / 100.0f));
        }

        //Flat healing, then calculated healing, then max hp
        else if (effect is HealingPart)
        {
            HealingPart trueEffect = effect as HealingPart;
            target.Heal(trueEffect.flatHealing);
            target.Heal(Mathf.RoundToInt((trueEffect.healing * caster.mAttack * 3.0f) / target.GetEffectiveDef()));
            target.Heal((int)(target.mHealth * trueEffect.maxHpPercent / 100.0f));
        }

        //Stat changes, self explanitory
        else if (effect is StatChangePart)
        {
            StatChangePart trueEffect = effect as StatChangePart;
            target.AddMod(trueEffect.StatMod);
        }

        //Adds or removes status effects
        else if (effect is StatusEffectPart)
        {
            StatusEffectPart trueEffect = effect as StatusEffectPart;
            if (trueEffect.remove)
                target.RemoveStatusEffect(trueEffect.status);
            else
                target.AddStatusEffect(trueEffect.status);
        }
    }

    /// <summary>
    /// Confirms the spell cast, casts the spell effects at every point in its AOE and checks for deaths
    /// </summary>
    public void ConfirmSkillCast()
    {
        skillCastConfirmMenu.SetActive(false);
        if (selectedMoveSpot.x != -1)
            ConfirmPlayerMove();
        Skill displaySkill = GameStorage.skillTreeList[GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].skillQuickList[selectedSpell - 1].x][GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].skillQuickList[selectedSpell - 1].y];
        CastSkill(displaySkill, spellCastPosition.x, spellCastPosition.y, GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]]);
        GameStorage.playerMasterList[GameStorage.activePlayerList[selectedPlayer]].moved = true;
        FinishedMovingPawn();
        CheckForDeath();
    }

    /// <summary>
    /// Cancels casting the spell through the spell cast confirm menu
    /// </summary>
    public void CancelSkillCast()
    {
        selectedEnemy = -1;
        spellCastPosition = new Vector2Int(-1, -1);
        skillCastConfirmMenu.SetActive(false);
    }

    #endregion
}
