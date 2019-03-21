using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The part of the battle currently being run
/// </summary>
public enum BattleState
{
    None,
    Swap,
    Player,
    Attack,
    Enemy
}

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
    DealPhysicalDamage,
    TakeMagicDamage,
    DealMagicDamage,
    BasicAttack,
    HitWithBasicAttack,
    SpellCast,
    DealSpellDamage,
    HealWithSpell,
    KillAnEnemy,
    Die,
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

    public EnemyMove(int x, int y, int priority, int reasonPriority)
    {
        movePosition = new Vector2Int(x, y);
        attackPosition = new Vector2Int(-1, -1);
        this.priority = priority;
        this.reasonPriority = reasonPriority;
    }
    public EnemyMove(int x, int y, int attackX, int attackY, int priority, int reasonPriority)
    {
        movePosition = new Vector2Int(x, y);
        attackPosition = new Vector2Int(attackX, attackY);
        this.priority = priority;
        this.reasonPriority = reasonPriority;
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

public class Battle : MonoBehaviour
{
    #region Data

    //Prefabs
    public GameObject EnemyBattleModelPrefab;
    public GameObject PlayerBattleModelPrefab;
    public GameObject MoveMarkerPrefab;
    public GameObject CameraPrefab;
    public GameObject battleTile;

    public GameObject skillCastConfirmMenu;

    public Vector2Int topLeft;

    public bool showDanger = false;
    public bool showaEther = false;

    //Battle state for the finite state machine
    public static BattleState battleState = BattleState.None;
    //Whether or not the players can swap positions, only true if no one has moved yet
    public bool canSwap;

    //Declares the map size, unchanged post initialization. Default is 20x20, camera will not change view to accomodate larger currently
    const int mapSizeX = 20;
    const int mapSizeY = 20;
    //Affects what the enemies take into account when making their moves, see MoveEnemies() for more information
    int difficulty;

    //Stores the physical tiles generated in the world to detect and interpret player input
    GameObject[,] tileList = new GameObject[20, 20];
    //Stores the data representation of the current chunk of the world, dictates where participants can move
    int[,] battleMap;
    //Stores the aEther levels of the area, slot 0 = current level, slot 1 = max level
    int[,,] aEtherMap;
    //Stores the players used in this battle
    public List<Player> players;
    //Stores the enemy data
    public List<Enemy> enemyList;
    //Stores the visual representation of the participants
    public Dictionary<BattleParticipant, GameObject> participantModels = new Dictionary<BattleParticipant, GameObject>();
    //This is a camera
    private GameObject battleCamera;
    public GameObject mapPlayer;

    //-1 means nothing selected
    public int selectedPlayer = -1;
    public int selectedEnemy = -1;
    public int hoveredSpell = -1;
    public int selectedSpell = -1;
    private int turn = 1;
    private Vector2Int selectedMoveSpot = new Vector2Int(-1, -1);
    private Vector2Int spellCastPosition = new Vector2Int(-1, -1);
    //This displays how the pawn would move when a move is selected
    public GameObject moveMarker;

    //What movement animation is currently running
    private MovementEvent currentAnimation;

    //A queue of all of the events that need to be run
    private BattleEventQueue eventQueue = new BattleEventQueue();

    //When it is the enemy's turn, keeps track of what enemy needs to be moved
    private int movingEnemy;

    //Whether a change has been made that would affect the states of one or more tiles
    //Keeps updateTiles from being called every frame
    private bool updateTilesThisFrame = false;

    #endregion

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
        //Removes whatever is left of the previous battle
        ExpungeAll();
        //Sets the map size, unused currently but functioning
        //mapSizeX = xSize;
        //mapSizeY = ySize;
        //Generates enemies
        //Grabs the map layout
        battleMap = GameStorage.GrabBattleMap(centerX, centerY, xSize, ySize);
        //Finds the top left corner of the current map
        topLeft = new Vector2Int(GameStorage.trueBX, GameStorage.trueBY);
        //Generates the visible tile map
        tileList = new GameObject[mapSizeX, mapSizeY];
        GenerateTileMap(topLeft.x, topLeft.y);
        //Grabs the aEther map
        aEtherMap = GameStorage.GrabaEtherMap(topLeft.x, topLeft.y, xSize, ySize);
        //Creates the move marker for the player
        moveMarker = Instantiate(MoveMarkerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
        //Make the camera
        battleCamera = Instantiate(CameraPrefab);
        skillCastConfirmMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        canSwap = true;
        //Sets up the opening camera animation
        eventQueue.Insert(new MovementEvent(battleCamera, 4f, battleCamera.transform.position, new Vector3(topLeft.x + (xSize / 2), 19, topLeft.y + (ySize / 2)), battleCamera.transform.rotation, battleCamera.transform.rotation, false));
        battleCamera.transform.SetPositionAndRotation(mainCamera.position, mainCamera.rotation);
        eventQueue.Insert(new FunctionEvent(ToMatch));
        players = new List<Player>();
        //Moves the player and enemy models into their correct position and sets up default values
        for (int i = 0; i < GameStorage.activePlayerList.Count; i++)
        {
            players.Add(GameStorage.playerMasterList[GameStorage.activePlayerList[i]]);
            players[i].position = new Vector2Int(6 + 2 * i, 10 + i % 2);
            players[i].StartOfMatch();
            CheckEventTriggers(players[i], EffectTriggers.StartOfMatch);
            CheckEventTriggers(players[i], EffectTriggers.StartOfTurn);
            participantModels.Add(players[i], Instantiate(PlayerBattleModelPrefab));
            participantModels[players[i]].transform.position = new Vector3(players[i].position.x + topLeft.x, 1, (mapSizeY - 1) - players[i].position.y + topLeft.y);
        }

        enemyList = new List<Enemy>();
        enemyList.Add(new Enemy(5, 5, 3, 5, 5));
        enemyList.Add(new Enemy(10, 5, 2, 5, 5));
        enemyList.Add(new Enemy(12, 5, 2, 5, 5));
        enemyList.Add(new Enemy(14, 5, 5, 5, 5));
        foreach (Enemy e in enemyList)
        {
            e.StartOfMatch();
            CheckEventTriggers(e, EffectTriggers.StartOfMatch);
            CheckEventTriggers(e, EffectTriggers.StartOfTurn);
            participantModels.Add(e, Instantiate(EnemyBattleModelPrefab));
            participantModels[e].transform.position = new Vector3(e.position.x + topLeft.x, 1, (mapSizeY - 1) - e.position.y + topLeft.y);
        }
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
    /// Update is called once per frame
    /// Controls the animations, the event queue, the battle's finite state machine and player input
    /// </summary>
    void Update()
    {
        if (skillCastConfirmMenu.activeSelf == false)
        {
            if (!currentAnimation.Equals(default(MovementEvent)))
            {
                if (currentAnimation.flatSpeed)
                {
                    currentAnimation.mover.transform.position += (currentAnimation.finalPosition - currentAnimation.initialPosition) * currentAnimation.speed * Time.deltaTime;
                    currentAnimation.mover.transform.rotation = Quaternion.Euler((currentAnimation.finalRotation.eulerAngles - currentAnimation.initialRotation.eulerAngles) * currentAnimation.speed * Time.deltaTime + currentAnimation.mover.transform.rotation.eulerAngles);//Vector3.Lerp(currentAnimation.initialRotation.eulerAngles, currentAnimation.finalRotation.eulerAngles, currentAnimation.speed) + currentAnimation.mover.transform.rotation.eulerAngles - currentAnimation.initialRotation.eulerAngles);
                }
                else
                {
                    currentAnimation.mover.transform.position = Vector3.Lerp(currentAnimation.mover.transform.position, currentAnimation.finalPosition, currentAnimation.speed * Time.deltaTime);
                    currentAnimation.mover.transform.rotation = Quaternion.Lerp(currentAnimation.mover.transform.rotation, currentAnimation.finalRotation, currentAnimation.speed * Time.deltaTime);
                }
                if (GameStorage.Approximately(currentAnimation.mover.transform.position, currentAnimation.finalPosition) && GameStorage.Approximately(currentAnimation.mover.transform.rotation, currentAnimation.finalRotation))
                {
                    currentAnimation.mover.transform.position = currentAnimation.finalPosition;
                    currentAnimation.mover.transform.rotation = currentAnimation.finalRotation;
                    currentAnimation = default(MovementEvent);
                    if (battleState != BattleState.None)
                        updateTilesThisFrame = true;
                }
            }
            else if (eventQueue.Count != 0)
            {
                BattleEventBase currentEvent = eventQueue.GetNext();
                if (currentEvent is ExecuteEffectEvent)
                {
                    ExecuteEffectEvent trueEvent = (ExecuteEffectEvent)currentEvent;
                    if (trueEvent.caster.cHealth > 0 || trueEvent.valueFromPrevious > -1)
                        ExecuteEffect(trueEvent.effect, trueEvent.caster, trueEvent.target, trueEvent.fromSpell, trueEvent.valueFromPrevious);
                }
                if (currentEvent is MovementEvent)
                    currentAnimation = (MovementEvent)currentEvent;
                if (currentEvent is TextEvent)
                    Debug.Log(((TextEvent)currentEvent).text);
                if (currentEvent is TurnEvent)
                {
                    TurnEvent trueEvent = (TurnEvent)currentEvent;
                    if (trueEvent.turner.cHealth > 0)
                    {
                        trueEvent.turner.facing = trueEvent.direction;
                        participantModels[trueEvent.turner].transform.rotation = Quaternion.Euler(0, 90 * (int)trueEvent.direction, 0);
                    }
                }
                if (currentEvent is FunctionEvent)
                    ((FunctionEvent)currentEvent).function();
            }
            else
            {
                switch (battleState)
                {
                    case BattleState.None:
                        break;
                    //Moves all the enemies one at a time until there are no more left to move
                    case BattleState.Enemy:
                        if (movingEnemy >= enemyList.Count)
                            eventQueue.Insert(new FunctionEvent(delegate { EndEnemyTurn(); }));
                        else
                        {
                            if (enemyList[movingEnemy].CanMove())
                                MoveEnemy(movingEnemy);
                            movingEnemy++;
                        }
                        break;
                    //Checks for the player clicking on a tile
                    case BattleState.Swap:
                    case BattleState.Player:
                    case BattleState.Attack:
                        //Sends out a raycast from where the player clicks that an only hit battle tiles
                        if (Input.GetMouseButtonDown(0))
                        {
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
                            updateTilesThisFrame = true;

                        break;
                }
            }
        }
        //If anything happened that could have changed the state of one or more tiles
        if (updateTilesThisFrame)
        {
            UpdateTileMap();
            updateTilesThisFrame = false;
        }
    }

    /// <summary>
    /// Deals with all of the possibilities of what the player could want to do when they click on a tile
    /// </summary>
    /// <param name="pos">What tile they clicked on</param>
    public void SpaceInteraction(Vector2Int pos)
    {
        //If the player should actually be able to interact with a tile
        if (battleState == BattleState.Swap || battleState == BattleState.Player || battleState == BattleState.Attack)
        {
            updateTilesThisFrame = true;

            bool actionTaken = false;
            switch (battleState)
            {
                case BattleState.Swap:
                    //If the player has already selected the first pawn in the swap and clicks on the second one
                    if (selectedPlayer != -1)
                    {
                        int n = PlayerAtPos(pos.x, pos.y);
                        players[n].position = players[selectedPlayer].position;
                        players[selectedPlayer].position = pos;
                        participantModels[players[n]].transform.position = new Vector3(players[n].position.x + topLeft.x, 1, (mapSizeY - 1) - players[n].position.y + topLeft.y);
                        participantModels[players[selectedPlayer]].transform.position = new Vector3(players[selectedPlayer].position.x + topLeft.x, 1, (mapSizeY - 1) - players[selectedPlayer].position.y + topLeft.y);
                        selectedPlayer = -1;
                        actionTaken = true;
                    }
                    break;
                case BattleState.Player:
                    //If player is trying to move a pawn
                    if (tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerMoveRange && !players[selectedPlayer].moved)
                    {
                        selectedMoveSpot.Set(pos.x, pos.y);
                        moveMarker.transform.position = new Vector3(pos.x + topLeft.x, 1, (mapSizeY - 1) - pos.y + topLeft.y);
                        moveMarker.SetActive(true);

                        //Update the line renderer
                        moveMarker.GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
                        Vector2Int moveDifference = new Vector2Int(selectedMoveSpot.x - players[selectedPlayer].position.x, selectedMoveSpot.y - players[selectedPlayer].position.y);

                        if (CanMoveYFirst(players[selectedPlayer], moveDifference))
                        {
                            moveMarker.GetComponent<LineRenderer>().SetPosition(1, new Vector3(-2 * moveDifference.x, 0, 0));
                            moveMarker.GetComponent<LineRenderer>().SetPosition(2, new Vector3(-2 * moveDifference.x, 0, 2 * moveDifference.y));
                        }
                        else
                        {
                            moveMarker.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 2 * moveDifference.y));
                            moveMarker.GetComponent<LineRenderer>().SetPosition(2, new Vector3(-2 * moveDifference.x, 0, 2 * moveDifference.y));
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
                //Selecting a different player
                if (battleState != BattleState.Attack)
                {
                    selectedPlayer = PlayerAtPos(pos.x, pos.y);
                    selectedMoveSpot = new Vector2Int(-1, -1);
                    moveMarker.SetActive(false);
                    selectedSpell = -1;
                }

                //Selecting an enemy
                selectedEnemy = EnemyAtPos(pos.x, pos.y);
                //If actually targetting another player for a healing attack
                if (selectedEnemy == -1 && tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerAttackRange && battleState == BattleState.Attack && selectedSpell == -1)
                    selectedEnemy = enemyList.Count + PlayerAtPos(pos.x, pos.y);
            }
        }
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
        foreach (GameObject obj in participantModels.Values)
        {
            Destroy(obj);
        }
        participantModels = new Dictionary<BattleParticipant, GameObject>();
        Destroy(battleCamera);
        battleCamera = null;
        Destroy(moveMarker);
        moveMarker = null;
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
            foreach (Player p in players)
            {
                CheckEventTriggers(p, EffectTriggers.EndOfMatch);
                p.EndOfMatch();
                p.GainExp(200);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            mapPlayer.SetActive(true);
            updateTilesThisFrame = false;
            eventQueue.Insert(new MovementEvent(mapPlayer.GetComponentInChildren<Camera>().gameObject, 4f, battleCamera.transform.position, mapPlayer.GetComponentInChildren<Camera>().transform.position, battleCamera.transform.rotation, mapPlayer.GetComponentInChildren<Camera>().transform.rotation, false));
            mapPlayer.GetComponentInChildren<Camera>().transform.SetPositionAndRotation(battleCamera.transform.position, battleCamera.transform.rotation);
            ExpungeAll();
        }
        else
        {
            battleState = BattleState.None;
            ExpungeAll();
        }
    }

    /// <summary>
    /// Toggles between swap and move at start of battle
    /// </summary>
    public void ToggleSwap()
    {
        if (canSwap)
        {
            battleState = (battleState == BattleState.Swap ? BattleState.Player : BattleState.Swap);
            selectedMoveSpot = new Vector2Int(-1, -1);
            selectedPlayer = -1;
            moveMarker.SetActive(false);
            updateTilesThisFrame = true;
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

    private void ToMatch()
    {
        battleState = BattleState.Player;
    }

    private void ToAttack()
    {
        battleState = BattleState.Attack;
    }

    #region PlayerTurn

    /// <summary>
    /// Sets up the movement animation to the position they want to go to for the player
    /// </summary>
    public void ConfirmPlayerMove()
    {
        if (selectedMoveSpot.x != -1)
        {
            Vector2Int diff = new Vector2Int(selectedMoveSpot.x - players[selectedPlayer].position.x, -(selectedMoveSpot.y - players[selectedPlayer].position.y));
            if (CanMoveYFirst(players[selectedPlayer], diff))
            {
                eventQueue.Insert(new TurnEvent(players[selectedPlayer], diff.y > 0 ? FacingDirection.North : FacingDirection.South));
                for (int y = 0; y < Mathf.Abs(diff.y); y++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 0.1f, participantModels[players[selectedPlayer]].transform.position + new Vector3(0, 0, y * Mathf.Sign(diff.y)), participantModels[players[selectedPlayer]].transform.position + new Vector3(0, 0, (y + 1) * Mathf.Sign(diff.y)), participantModels[players[selectedPlayer]].transform.rotation, participantModels[players[selectedPlayer]].transform.rotation, false));
                }
                eventQueue.Insert(new TurnEvent(players[selectedPlayer], diff.x > 0 ? FacingDirection.East : FacingDirection.West));
                for (int x = 0; x < Mathf.Abs(diff.x); x++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 0.1f, participantModels[players[selectedPlayer]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0, diff.y), participantModels[players[selectedPlayer]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0, diff.y), participantModels[players[selectedPlayer]].transform.rotation, participantModels[players[selectedPlayer]].transform.rotation, false));
                }
            }
            else
            {
                eventQueue.Insert(new TurnEvent(players[selectedPlayer], diff.x > 0 ? FacingDirection.East : FacingDirection.West));
                for (int x = 0; x < Mathf.Abs(diff.x); x++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 0.1f, participantModels[players[selectedPlayer]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0), participantModels[players[selectedPlayer]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0), participantModels[players[selectedPlayer]].transform.rotation, participantModels[players[selectedPlayer]].transform.rotation, false));
                }
                eventQueue.Insert(new TurnEvent(players[selectedPlayer], diff.y > 0 ? FacingDirection.North : FacingDirection.South));
                for (int y = 0; y < Mathf.Abs(diff.y); y++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 0.1f, participantModels[players[selectedPlayer]].transform.position + new Vector3(diff.x, 0, y * Mathf.Sign(diff.y)), participantModels[players[selectedPlayer]].transform.position + new Vector3(diff.x, 0, (y + 1) * Mathf.Sign(diff.y)), participantModels[players[selectedPlayer]].transform.rotation, participantModels[players[selectedPlayer]].transform.rotation, false));
                }
            }
            players[selectedPlayer].position += new Vector2Int(diff.x, -diff.y);
            selectedMoveSpot = new Vector2Int(-1, -1);
            moveMarker.SetActive(false);
            canSwap = false;
            eventQueue.Insert(new FunctionEvent(ToAttack));
        }
    }

    /// <summary>
    /// Sets up an attack from a player pawn on an enemy, or healing from a player pawn to another player pawn
    /// </summary>
    public void PerformPlayerAttack()
    {
        //If healing a player
        if (selectedEnemy >= enemyList.Count)
            eventQueue.Insert(new ExecuteEffectEvent(new HealingPart(TargettingType.Ally, players[selectedPlayer].GetEffectiveMAtk() / 2, 0, 0), players[selectedPlayer], players[selectedEnemy - enemyList.Count]));
        //If attacking an enemy
        else
            PerformAttack(players[selectedPlayer], enemyList[selectedEnemy]);
        FinishedMovingPawn();
    }

    /// <summary>
    /// Sets up for the next pawn to be moved
    /// or
    /// If all player pawns have finished moving, set up for enemies to move
    /// </summary>
    public void FinishedMovingPawn()
    {
        players[selectedPlayer].moved = true;
        selectedMoveSpot = new Vector2Int(-1, -1);
        selectedPlayer = -1;
        selectedEnemy = -1;
        selectedSpell = -1;
        updateTilesThisFrame = true;

        //Checks if all players are done moving
        bool playersDone = true;
        foreach (Player p in players)
        {
            if (!p.moved)
                playersDone = false;
        }
        if (playersDone)
        {
            //Resets to start enemy moves
            for (int j = 0; j < enemyList.Count; j++)
            {
                enemyList[j].moved = !enemyList[j].CanMove();
            }
            battleState = BattleState.Enemy;
        }
    }

    /// <summary>
    /// If player wants to end the turn before all ally pawns have been moved
    /// </summary>
    public void EndPlayerTurnEarly()
    {
        moveMarker.SetActive(false);
        canSwap = false;
        foreach (Player p in players)
        {
            p.moved = true;
        }
        FinishedMovingPawn();
    }

    #endregion

    #region EnemyTurn

    /// <summary>
    /// Finds the optimal move for the enemy currently moving
    /// </summary>
    private void MoveEnemy(int ID)
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

        List<EnemyMove> possibleMoves = new List<EnemyMove>();
        EnemyMove fallbackMove = new EnemyMove(0, 0, 100, 0);
        List<Vector2Int> moveSpots = GetViableMovements(enemyList[ID]);
        WeaponType weapon = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[enemyList[ID].equippedWeapon.Name]).subType];
        foreach (Vector2Int pos in moveSpots)
        {
            List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, new Vector2Int(pos.x, pos.y));
            foreach (Vector2Int aPos in attackSpots)
            {
                if (PlayerAtPos(aPos.x, aPos.y) != -1)
                {
                    possibleMoves.Add(new EnemyMove(pos.x, pos.y, aPos.x, aPos.y, 15 - (Mathf.Abs(pos.x - enemyList[ID].position.x) + Mathf.Abs(pos.y - enemyList[ID].position.y)), 1));
                }
            }
            foreach (Player p in players)
            {
                //If this move is the closest the enemy can get to a player, make it the move that happens if no attacks are possible
                if (Mathf.Abs(p.position.x - pos.x) + Mathf.Abs(p.position.y - pos.y) < fallbackMove.priority)
                {
                    fallbackMove = new EnemyMove(pos.x, pos.y, Mathf.Abs(p.position.x - pos.x) + Mathf.Abs(p.position.y - pos.y), 0);
                }
                //If this move ties with the current fallback move for distance from a player, pick a random one
                else if (Mathf.Abs(p.position.x - pos.x) + Mathf.Abs(p.position.y - pos.y) == fallbackMove.priority)
                {
                    if (Random.Range(0, 2) == 1)
                        fallbackMove = new EnemyMove(pos.x, pos.y, Mathf.Abs(p.position.x - pos.x) + Mathf.Abs(p.position.y - pos.y), 0);
                }
            }
        }
        //If the enemy can't attack anyone, adds the move that would get them closest to the nearest player
        if (possibleMoves.Count == 0)
            possibleMoves.Add(fallbackMove);
        else
        {
            //Sorts the possible moves in order of priority
            possibleMoves.Sort(delegate (EnemyMove c1, EnemyMove c2) { return c1.CompareTo(c2); });

            //Chooses between moves with equal priority
            while (possibleMoves.Count > 1 && possibleMoves[0].CompareTo(possibleMoves[1]) == 0)
            {
                possibleMoves.RemoveAt(Random.Range(0, 2));
            }
        }

        //Sets up the animations for moving the enemy
        Vector2Int diff = new Vector2Int(possibleMoves[0].movePosition.x - enemyList[ID].position.x, -(possibleMoves[0].movePosition.y - enemyList[ID].position.y));

        if (CanMoveYFirst(enemyList[ID], diff))
        {
            eventQueue.Insert(new TurnEvent(enemyList[ID], diff.y > 0 ? FacingDirection.North : FacingDirection.South));
            for (int y = 0; y < Mathf.Abs(diff.y); y++)
            {
                eventQueue.Insert(new MovementEvent(participantModels[enemyList[ID]], 0.1f, participantModels[enemyList[ID]].transform.position + new Vector3(0, 0, y * Mathf.Sign(diff.y)), participantModels[enemyList[ID]].transform.position + new Vector3(0, 0, (y + 1) * Mathf.Sign(diff.y)), participantModels[enemyList[ID]].transform.rotation, participantModels[enemyList[ID]].transform.rotation, false));
            }
            eventQueue.Insert(new TurnEvent(enemyList[ID], diff.x > 0 ? FacingDirection.East : FacingDirection.West));
            for (int x = 0; x < Mathf.Abs(diff.x); x++)
            {
                eventQueue.Insert(new MovementEvent(participantModels[enemyList[ID]], 0.1f, participantModels[enemyList[ID]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0, diff.y), participantModels[enemyList[ID]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0, diff.y), participantModels[enemyList[ID]].transform.rotation, participantModels[enemyList[ID]].transform.rotation, false));
            }
        }
        else
        {
            eventQueue.Insert(new TurnEvent(enemyList[ID], diff.x > 0 ? FacingDirection.East : FacingDirection.West));
            for (int x = 0; x < Mathf.Abs(diff.x); x++)
            {
                eventQueue.Insert(new MovementEvent(participantModels[enemyList[ID]], 0.1f, participantModels[enemyList[ID]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0), participantModels[enemyList[ID]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0), participantModels[enemyList[ID]].transform.rotation, participantModels[enemyList[ID]].transform.rotation, false));
            }
            eventQueue.Insert(new TurnEvent(enemyList[ID], diff.y > 0 ? FacingDirection.North : FacingDirection.South));
            for (int y = 0; y < Mathf.Abs(diff.y); y++)
            {
                eventQueue.Insert(new MovementEvent(participantModels[enemyList[ID]], 0.1f, participantModels[enemyList[ID]].transform.position + new Vector3(diff.x, 0, y * Mathf.Sign(diff.y)), participantModels[enemyList[ID]].transform.position + new Vector3(diff.x, 0, (y + 1) * Mathf.Sign(diff.y)), participantModels[enemyList[ID]].transform.rotation, participantModels[enemyList[ID]].transform.rotation, false));
            }
        }
        enemyList[ID].position += new Vector2Int(diff.x, -diff.y);
        enemyList[ID].moved = true;
        if (possibleMoves[0].attackPosition.x != -1)
            PerformAttack(enemyList[movingEnemy], players[PlayerAtPos(possibleMoves[0].attackPosition.x, possibleMoves[0].attackPosition.y)]);
        updateTilesThisFrame = true;
    }

    /// <summary>
    /// Ends the enemy's turn and sets up the player's turn
    /// </summary>
    private void EndEnemyTurn()
    {
        //Resets to allow players to move and starts player's turn
        foreach (Player p in players)
        {
            if (p.cHealth > 0)
            {
                p.EndOfTurn();
                CheckEventTriggers(p, EffectTriggers.EndOfTurn);
                p.StartOfTurn();
                CheckEventTriggers(p, EffectTriggers.StartOfTurn);
                p.moved = !p.CanMove();
            }
        }
        foreach (Enemy e in enemyList)
        {
            if (e.cHealth > 0)
            {
                e.EndOfTurn();
                CheckEventTriggers(e, EffectTriggers.EndOfTurn);
                e.StartOfTurn();
                CheckEventTriggers(e, EffectTriggers.StartOfTurn);
            }
        }
        movingEnemy = 0;
        turn++;
        battleState = BattleState.Player;
    }

    #endregion

    /// <summary>
    /// Checks to see if all of one team is dead and triggers OnBattleEnd if so
    /// </summary>
    public void CheckForDeath()
    {
        int deadCount = 0;
        foreach (Player p in players)
        {
            if (p.cHealth <= 0)
            {
                deadCount++;
                participantModels[p].SetActive(false);
                p.position.Set(-200, -200);
            }
        }
        if (deadCount == players.Count)
            OnBattleEnd(false);
        deadCount = 0;
        foreach (Enemy e in enemyList)
        {
            if (e.cHealth <= 0)
            {
                deadCount++;
                participantModels[e].SetActive(false);
                e.position.Set(-200, -200);
            }
        }
        if (deadCount == enemyList.Count)
            OnBattleEnd(true);
    }

    /// <summary>
    /// Gets how much damage attacker would do when attacking target
    /// </summary>
    /// <param name="attacker">The pawn doing the attacking</param>
    /// <param name="target">The pawn getting attacked</param>
    /// <returns>The first value is the amount of damage, the second is the damage type and whether or not the weapon is ranged</returns>
    public Triple<int, DamageType, bool> GetDamageValues(BattleParticipant attacker, BattleParticipant target)
    {
        //Gets the distance between the player and enemy
        int dist = Mathf.Abs(target.position.x - attacker.position.x);
        if (Mathf.Abs(target.position.y - attacker.position.y) > dist)
            dist = Mathf.Abs(target.position.y - attacker.position.y);

        DamageType type = ((EquippableBase)Registry.ItemRegistry[attacker.equippedWeapon.Name]).statType;
        float mod = 1.0f;
        bool ranged = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[attacker.equippedWeapon.Name]).subType].ranged;
        foreach (RangeDependentAttack r in Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[attacker.equippedWeapon.Name]).subType].specialRanges)
        {
            if (r.atDistance == dist)
            {
                type = r.damageType;
                mod = r.damageMult;
                ranged = r.ranged;
                break;
            }
        }

        //Checks for a critical hit, which multiplies damage by 1.5
        mod *= Random.Range(0, 100) < attacker.critChance + ((EquippableBase)Registry.ItemRegistry[attacker.equippedWeapon.Name]).critChanceMod ? 1.5f : 1.0f;

        if (type == DamageType.Physical)
            return new Triple<int, DamageType, bool>(Mathf.RoundToInt((attacker.GetEffectiveAtk(dist) * mod * 3.0f) / target.GetEffectiveDef(dist)), type, ranged);
        else
            return new Triple<int, DamageType, bool>(Mathf.RoundToInt((attacker.GetEffectiveMAtk(dist) * mod * 3.0f) / target.GetEffectiveMDef(dist)), type, ranged);
    }

    /// <summary>
    /// Performs attack, then checks for an executes possible counterattack if both pawns are still alive
    /// </summary>
    /// <param name="attacker">The pawn that is initially attacking</param>
    /// <param name="defender">The pawn that is initially defending</param>
    public void PerformAttack(BattleParticipant attacker, BattleParticipant defender)
    {
        Triple<int, DamageType, bool> attackData = GetDamageValues(attacker, defender);
        int damage = defender.Damage(Mathf.RoundToInt(attackData.First));
        CheckEventTriggers(attacker, EffectTriggers.BasicAttack, defender, damage);
        CheckEventTriggers(defender, EffectTriggers.HitWithBasicAttack, attacker, damage);

        eventQueue.Insert(new ExecuteEffectEvent(new DamagePart(TargettingType.Enemy, attackData.Second, 0, attackData.First, 0, 0), attacker, defender));

        //If the defender lives
        if (defender.cHealth > 0)
        {
            Triple<int, DamageType, bool> counterattackData = GetDamageValues(defender, attacker);
            //If the two weapons are both melee or both ranged there can be a counterattack
            if (attackData.Third == counterattackData.Third)
            {
                damage = attacker.Damage(Mathf.RoundToInt(counterattackData.First));
                CheckEventTriggers(defender, EffectTriggers.BasicAttack, attacker, damage);
                CheckEventTriggers(attacker, EffectTriggers.HitWithBasicAttack, defender, damage);

                eventQueue.Insert(new ExecuteEffectEvent(new DamagePart(TargettingType.Enemy, counterattackData.Second, 0, counterattackData.First, 0, 0), defender, attacker));
            }
        }
    }

    /// <summary>
    /// Checks if there is an player at the given x and y values
    /// </summary>
    private int PlayerAtPos(int x, int y)
    {
        for (int id = 0; id < players.Count; id++)
        {
            if (players[id].position.x == x && players[id].position.y == y)
                return id;
        }
        return -1;
    }

    /// <summary>
    /// Checks if there is an enemy at the given x and y values
    /// </summary>
    private int EnemyAtPos(int x, int y)
    {
        for (int e = 0; e < enemyList.Count; e++)
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
        //Resets the tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileList[x, y].GetComponent<BattleTile>().Reset();

                //Updates the aEther viewer
                tileList[x, y].GetComponentsInChildren<Renderer>()[1].enabled = showaEther;
                if (showaEther)
                    tileList[x, y].GetComponentsInChildren<Transform>()[1].localScale = new Vector3(0.1f * aEtherMap[x, y, 0], 0.01f, 0.1f * aEtherMap[x, y, 0]);
            }
        }

        if (battleState == BattleState.Swap)
        {
            foreach (Player p in players)
            {
                tileList[p.position.x, (mapSizeY - 1) - p.position.y].GetComponent<BattleTile>().playerMoveRange = true;
            }
            return;
        }

        //Shows skill range and what is targettable within that range if a spell is selected or hovered
        if (selectedSpell != -1 || hoveredSpell != -1)
        {
            int skillToShow = selectedSpell;
            if (hoveredSpell != -1)
                skillToShow = hoveredSpell;
            Vector2Int skillPos = players[selectedPlayer].position;
            if (selectedMoveSpot.x != -1)
                skillPos = selectedMoveSpot;
            Skill displaySkill = GameStorage.skillTreeList[players[selectedPlayer].skillQuickList[skillToShow - 1].x][players[selectedPlayer].skillQuickList[skillToShow - 1].y];

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
                            if (displaySkill.targetType == TargettingType.AllInRange || displaySkill.targetType == TargettingType.Ally && PlayerAtPos(x + skillPos.x, y + skillPos.y) != -1 || displaySkill.targetType == TargettingType.Enemy && EnemyAtPos(x + skillPos.x, y + skillPos.y) != -1)
                                tileList[x + skillPos.x, (mapSizeY - 1) - (y + skillPos.y)].GetComponent<BattleTile>().skillTargettable = true;
                            else
                                tileList[x + skillPos.x, (mapSizeY - 1) - (y + skillPos.y)].GetComponent<BattleTile>().skillRange = true;
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
                    Vector2Int pos = hit.transform.GetComponent<BattleTile>().arrayID;
                    BattleTile.skillLegitTarget = tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().skillTargettable;
                    for (int x = -Mathf.FloorToInt((displaySkill.xRange - 1) / 2.0f); x <= Mathf.CeilToInt((displaySkill.xRange - 1) / 2.0f); x++)
                    {
                        for (int y = -Mathf.FloorToInt((displaySkill.yRange - 1) / 2.0f); y <= Mathf.CeilToInt((displaySkill.yRange - 1) / 2.0f); y++)
                        {
                            if (x + pos.x >= 0 && x + pos.x < mapSizeX && y + pos.y >= 0 && y + pos.y < mapSizeY)
                                tileList[x + pos.x, (mapSizeY - 1) - (y + pos.y)].GetComponent<BattleTile>().skillTargetting = true;
                        }
                    }
                }
            }
        }
        //Renders where a valid attack spot would be for a player after they move
        else if (battleState == BattleState.Attack)
        {
            WeaponType weapon = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[players[selectedPlayer].equippedWeapon.Name]).subType];

            List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, players[selectedPlayer].position);
            foreach (Vector2Int attackPos in attackSpots)
            {
                if ((EnemyAtPos(attackPos.x, attackPos.y) == -1 || ((EquippableBase)Registry.ItemRegistry[players[selectedPlayer].equippedWeapon.Name]).strength == 0) && (PlayerAtPos(attackPos.x, attackPos.y) == -1 || !weapon.heals))
                    tileList[attackPos.x, (mapSizeY - 1) - attackPos.y].GetComponent<BattleTile>().playerAttackRange = true;
            }
        }
        //Renders the possible movements for a given player-controlled pawn
        else if (battleState == BattleState.Player && selectedPlayer != -1)
        {
            List<Vector2Int> moveSpots = GetViableMovements(players[selectedPlayer]);
            WeaponType weapon = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[players[selectedPlayer].equippedWeapon.Name]).subType];
            foreach (Vector2Int pos in moveSpots)
            {
                tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerMoveRange = true;
                List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, pos);
                foreach (Vector2Int attackPos in attackSpots)
                {
                    tileList[attackPos.x, (mapSizeY - 1) - attackPos.y].GetComponent<BattleTile>().playerAttackRange = true;
                }
            }
        }

        //Renders the movement and attack ranges of enemies if requested
        if (showDanger)
        {
            foreach (Enemy e in enemyList)
            {
                List<Vector2Int> moveSpots = GetViableMovements(e);
                WeaponType weapon = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[e.equippedWeapon.Name]).subType];
                foreach (Vector2Int pos in moveSpots)
                {
                    tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().enemyDanger = true;
                    List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, pos);
                    foreach (Vector2Int attackPos in attackSpots)
                    {
                        tileList[attackPos.x, (mapSizeY - 1) - attackPos.y].GetComponent<BattleTile>().enemyDanger = true;
                    }
                }
            }
        }

        //Updates the colors of all the tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tileList[x, y].GetComponent<BattleTile>().UpdateColors();
            }
        }
    }

    /// <summary>
    /// Returns whether or not the path a pawn can take to a given position can be vertical first
    /// </summary>
    /// <param name="mover">The pawn that is moving</param>
    /// <param name="relativeMove">The position they want to move to relative to their current position</param>
    private bool CanMoveYFirst(BattleParticipant mover, Vector2Int relativeMove)
    {
        for (int y = 0; y <= Mathf.Abs(relativeMove.y); y++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.position.x, mover.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))]))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.position.x, mover.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))) : PlayerAtPos(mover.position.x, mover.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y)))) != -1)
                return false;
        }

        for (int x = 0; x <= Mathf.Abs(relativeMove.x); x++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.position.y + relativeMove.y]))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.position.y + relativeMove.y) : PlayerAtPos(mover.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.position.y + relativeMove.y)) != -1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets all of the places a given pawn can move to
    /// </summary>
    /// <param name="entity">The entity moving</param>
    /// <returns>First = a valid position, Second = whether or not moving vertically first is valid</returns>
    private List<Vector2Int> GetViableMovements(BattleParticipant entity)
    {
        List<Vector2Int> moveSpots = new List<Vector2Int>();
        bool isPlayer = entity is Player;
        int maxMove = entity.GetMoveSpeed();
        for (int x = -maxMove; x <= maxMove; x++)
        {
            for (int y = -maxMove; y <= maxMove; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= maxMove && x + entity.position.x >= 0 && x + entity.position.x < mapSizeX && y + entity.position.y >= 0 && y + entity.position.y < mapSizeY)
                {
                    //It is automatically valid if the entity is moving to itself
                    if (x == 0 && y == 0)
                    {
                        moveSpots.Add(new Vector2Int(entity.position.x, entity.position.y));
                        continue;
                    }
                    //It is an invalid move position if it would overlap with an existing entity
                    if (PlayerAtPos(x + entity.position.x, y + entity.position.y) != -1)
                        continue;
                    if (EnemyAtPos(x + entity.position.x, y + entity.position.y) != -1)
                        continue;

                    bool goodTile = true;
                    //If it passes all of the preliminary tests
                    if (goodTile)
                    {
                        //Check every tile along the path, starting by moving vertically
                        for (int cY = 1; cY <= Mathf.Abs(y); cY++)
                        {
                            if (!entity.ValidMoveTile(battleMap[entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y]))
                                goodTile = false;
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
                                    goodTile = false;
                                //Checks if a member of the opposing team is in the way
                                if (isPlayer ? EnemyAtPos(cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, y + entity.position.y) != -1 : PlayerAtPos(cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, y + entity.position.y) != -1)
                                    goodTile = false;
                            }
                            //If it passes the y-first set of tests, mark it as a valid move tile
                            if (goodTile)
                            {
                                moveSpots.Add(new Vector2Int(x + entity.position.x, y + entity.position.y));
                                continue;
                            }
                        }
                        //if invalid by y, x, check x, y
                        goodTile = true;
                        //Check every tile along the path, starting by moving horizontally
                        for (int cX = 1; cX <= Mathf.Abs(x); cX++)
                        {
                            if (!entity.ValidMoveTile(battleMap[cX * Mathf.RoundToInt(Mathf.Sign(x)) + entity.position.x, entity.position.y]))
                                goodTile = false;
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
                                    goodTile = false;
                                //Checks if a member of the opposing team is in the way
                                if (isPlayer ? EnemyAtPos(x + entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y) != -1 : PlayerAtPos(x + entity.position.x, cY * Mathf.RoundToInt(Mathf.Sign(y)) + entity.position.y) != -1)
                                    goodTile = false;
                            }
                            //If it passes the x-first set of tests, mark it as a valid move tile
                            if (goodTile)
                            {
                                moveSpots.Add(new Vector2Int(x + entity.position.x, y + entity.position.y));
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
                    attackSpots.Add(new Vector2Int(center.x - i, center.y + i));

                if (center.x + i < mapSizeX && battleMap[center.x + i, (mapSizeY - 1) - (center.y + 1)] != 6)
                    attackSpots.Add(new Vector2Int(center.x + i, center.y + i));
            }
            if (center.y - i >= 0)
            {
                if (center.x - i >= 0 && battleMap[center.x - i, (mapSizeY - 1) - (center.y - 1)] != 6)
                    attackSpots.Add(new Vector2Int(center.x - i, center.y - i));

                if (center.x + i < mapSizeX && battleMap[center.x + i, (mapSizeY - 1) - (center.y - 1)] != 6)
                    attackSpots.Add(new Vector2Int(center.x + i, center.y - i));
            }
        }
        return attackSpots;
    }

    /// <summary>
    /// Checks for and executes all events triggered by the given trigger
    /// </summary>
    /// <param name="triggered">The pawn we are checking</param>
    /// <param name="trigger">The type of trigger that was tripped</param>
    /// <param name="other">If the event involved another pawn, such as taking damage or giving healing</param>
    /// <param name="data">If the event has other important data, such as the amount of damage taken</param>
    private void CheckEventTriggers(BattleParticipant triggered, EffectTriggers trigger, BattleParticipant other = null, int data = -1)
    {
        List<SkillPartBase> list = triggered.GetTriggeredEffects(trigger);
        foreach (SkillPartBase effect in list)
        {
            if (effect.targetType == TargettingType.Self || effect.targetType == TargettingType.AllAllies)
                eventQueue.Insert(new ExecuteEffectEvent(effect, triggered, triggered, false, data));

            bool isPlayer = triggered is Player;
            if (isPlayer && (effect.targetType == TargettingType.AllAlliesNotSelf || effect.targetType == TargettingType.AllAllies) || !isPlayer && effect.targetType == TargettingType.AllEnemies)
            {
                foreach (Player p in players)
                {
                    if (p != triggered && p.cHealth > 0)
                        eventQueue.Insert(new ExecuteEffectEvent(effect, triggered, p, false, data));
                }
            }
            else if (!isPlayer && (effect.targetType == TargettingType.AllAlliesNotSelf || effect.targetType == TargettingType.AllAllies) || isPlayer && effect.targetType == TargettingType.AllEnemies)
            {
                foreach (Enemy enemy in enemyList)
                {
                    if (enemy != triggered && enemy.cHealth > 0)
                        eventQueue.Insert(new ExecuteEffectEvent(effect, triggered, enemy, false, data));
                }
            }
            else if (effect.targetType != TargettingType.Self)
            {
                eventQueue.Insert(new ExecuteEffectEvent(effect, triggered, other, false, data));
            }
        }
    }

    /// <summary>
    /// Executes the effects of a skill part on a pawn
    /// </summary>
    /// <param name="effect">The skill part to execute</param>
    /// <param name="caster">The caster of the skill</param>
    /// <param name="target">The target for the skill</param>
    /// <param name="fromSpell">Whether this effect is part of a spell or not</param>
    /// <param name="valueFromPrevious">If this effect depends on the value from a previous event, this is the value. -1 if it doesn't depend on anything.</param>
    public void ExecuteEffect(SkillPartBase effect, BattleParticipant caster, BattleParticipant target, bool fromSpell = false, int valueFromPrevious = -1)
    {
        //If it passes its chance to proc
        if (Random.Range(0, 101) <= effect.chanceToProc)
        {
            //Flat damage, then calculated damage, then remaining hp, then max hp
            if (effect is DamagePart)
            {
                DamagePart trueEffect = effect as DamagePart;
                int previousHealth = target.cHealth;
                //If this is supposed to be modified by a previous value, do so. Otherwise, don't
                float previousValueMod = trueEffect.modifiedByValue != 0 ? trueEffect.modifiedByValue * valueFromPrevious : 1;
                int damage = target.Damage(Mathf.RoundToInt(trueEffect.flatDamage * previousValueMod));
                damage += target.Damage(Mathf.RoundToInt((trueEffect.damage * previousValueMod * (trueEffect.damageType == DamageType.Physical ? caster.GetEffectiveAtk() : caster.GetEffectiveMAtk()) * 3.0f) / (trueEffect.damageType == DamageType.Physical ? caster.GetEffectiveDef() : caster.GetEffectiveMDef())));
                damage += target.Damage((int)(target.cHealth * trueEffect.remainingHpPercent * previousValueMod / 100.0f));
                damage += target.Damage((int)(target.mHealth * trueEffect.maxHpPercent * previousValueMod / 100.0f));
                //Keeps effects from stacking on top of each other by a looped damage call
                if (trueEffect.modifiedByValue == 0)
                {
                    CheckEventTriggers(caster, EffectTriggers.DealDamage, target, damage);
                    CheckEventTriggers(target, EffectTriggers.TakeDamage, caster, damage);
                    if (fromSpell)
                        CheckEventTriggers(caster, EffectTriggers.DealSpellDamage, target, damage);

                    if (trueEffect.damageType == DamageType.Physical)
                    {
                        CheckEventTriggers(caster, EffectTriggers.DealPhysicalDamage, target, damage);
                        CheckEventTriggers(target, EffectTriggers.TakePhysicalDamage, caster, damage);
                    }
                    else
                    {
                        CheckEventTriggers(caster, EffectTriggers.DealMagicDamage, target, damage);
                        CheckEventTriggers(target, EffectTriggers.TakeMagicDamage, caster, damage);
                    }
                }

                //If this causes them to fall below 50% health
                if (previousHealth >= target.mHealth / 2.0 && target.cHealth < target.mHealth / 2.0)
                    CheckEventTriggers(target, EffectTriggers.FallBelow50Percent, caster);
                //If this causes them to fall below 50% health
                if (previousHealth >= target.mHealth / 4.0 && target.cHealth < target.mHealth / 4.0)
                    CheckEventTriggers(target, EffectTriggers.FallBelow25Percent, caster);
                //If anyone dies
                if (target.cHealth <= 0 || caster.cHealth <= 0)
                {
                    CheckEventTriggers(target, EffectTriggers.Die);
                    //If anyone is still dead
                    if (target.cHealth <= 0 || caster.cHealth <= 0)
                    {
                        CheckEventTriggers(target, EffectTriggers.KillAnEnemy);
                        eventQueue.Insert(new FunctionEvent(delegate { CheckForDeath(); }));
                    }
                }
            }

            //Flat healing, then calculated healing, then max hp
            else if (effect is HealingPart)
            {
                HealingPart trueEffect = effect as HealingPart;
                //If this is supposed to be modified by a previous value, do so. Otherwise, don't
                float previousValueMod = trueEffect.modifiedByValue != 0 ? trueEffect.modifiedByValue * valueFromPrevious : 1;
                int healing = target.Heal(Mathf.RoundToInt(trueEffect.flatHealing * previousValueMod));
                healing += target.Heal(Mathf.RoundToInt((trueEffect.healing * previousValueMod * caster.GetEffectiveMAtk() * 3.0f) / target.GetEffectiveDef()));
                healing += target.Heal((int)(target.mHealth * trueEffect.maxHpPercent * previousValueMod / 100.0f));
                //Keeps effects from stacking on top of each other by a looped healing call
                if (trueEffect.modifiedByValue == 0)
                {
                    CheckEventTriggers(caster, EffectTriggers.Healing, target, healing);
                    CheckEventTriggers(target, EffectTriggers.GettingHealed, caster, healing);
                    if (fromSpell)
                        CheckEventTriggers(caster, EffectTriggers.HealWithSpell, target, healing);
                }
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

            //Adds a temporary trigger to the target
            else if (effect is AddTriggerPart)
            {
                AddTriggerPart trueEffect = effect as AddTriggerPart;
                target.AddTemporaryTrigger(trueEffect.effect, trueEffect.turnLimit);
            }

            //Moves the target in a given direction for a maximum of the given spaces
            else if (effect is MovePart)
            {
                MovePart trueEffect = effect as MovePart;
                MoveDirection direction = trueEffect.direction;

                //Randomizes the movement direction if that's what is needed
                if (direction == MoveDirection.Random)
                    direction = (MoveDirection)Random.Range(0, 4);

                //If the target is being pushed away from the center, figure out what direction they should be moved in
                if (direction == MoveDirection.FromCenter)
                {
                    Vector2Int diff = target.position - trueEffect.center;
                    //If they are equal, prioritizes y
                    if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                    {
                        if (diff.x > 0)
                            direction = MoveDirection.Right;
                        direction = MoveDirection.Left;
                    }
                    else
                    {
                        if (diff.y > 0)
                            direction = MoveDirection.Down;
                        direction = MoveDirection.Up;
                    }
                }

                if (direction == MoveDirection.Up || direction == MoveDirection.Down)
                {
                    int dir = direction == MoveDirection.Up ? 1 : -1;
                    for (int i = 1; i <= trueEffect.amount; i++)
                    {
                        if (target.ValidMoveTile(battleMap[target.position.x, target.position.y + i * dir]))
                            eventQueue.Insert(new MovementEvent(participantModels[target], 0.1f, participantModels[target].transform.position + new Vector3Int(0, 0, i - 1) * dir, participantModels[target].transform.position + new Vector3Int(0, 0, i) * dir, participantModels[target].transform.rotation, participantModels[target].transform.rotation, true));
                        else
                        {
                            //If the pawn hits a spot where they can't move any further, stun them for a turn
                            eventQueue.Insert(new ExecuteEffectEvent(new StatusEffectPart(TargettingType.AllInRange, "stun", false), caster, target));
                            return;
                        }
                    }
                }
                //If it is right or left
                else
                {
                    int dir = direction == MoveDirection.Right ? 1 : -1;
                    for (int i = 1; i <= trueEffect.amount; i++)
                    {
                        if (target.ValidMoveTile(battleMap[target.position.x + i * dir, target.position.y]))
                            eventQueue.Insert(new MovementEvent(participantModels[target], 0.1f, participantModels[target].transform.position + new Vector3Int(i - 1, 0, 0) * dir, participantModels[target].transform.position + new Vector3Int(i, 0, 0) * dir, participantModels[target].transform.rotation, participantModels[target].transform.rotation, true));
                        else
                        {
                            //If the pawn hits a spot where they can't move any further, stun them for a turn
                            eventQueue.Insert(new ExecuteEffectEvent(new StatusEffectPart(TargettingType.AllInRange, "stun", false), caster, target));
                            return;
                        }
                    }
                }
            }

            else if (effect is UniqueEffectPart)
            {
                switch ((effect as UniqueEffectPart).effect)
                {
                    //Makes that pawn able to move again if it can move at all
                    case UniqueEffects.MoveAgain:
                        //If the target cannot already move again
                        if (!target.moved)
                        {
                            //Check if the target is able to move again
                            bool canMove = target.CanMove();
                            target.moved = !canMove;
                            //If it is an enemy that can move again, moves back through the enemy list and tries to move them again
                            if (battleState == BattleState.Enemy && canMove)
                            {
                                movingEnemy = 0;
                            }
                        }
                        break;
                }
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
    /// Called when the player selects a spell they want to try and cast from their quick cast list
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

        CheckEventTriggers(caster, EffectTriggers.SpellCast);

        foreach (SkillPartBase effect in castedSpell.partList)
        {
            //If the effect is a movement effect and is supposed to push away from the cast position, update the cast position
            if (effect is MovePart && (effect as MovePart).direction == MoveDirection.FromCenter)
                (effect as MovePart).center = new Vector2Int(castedX, castedY);

            if (effect.targetType == TargettingType.Self || effect.targetType == TargettingType.AllAllies)
                eventQueue.Insert(new ExecuteEffectEvent(effect, caster, caster, true));

            bool castedByPlayer = caster is Player;
            //If it is a player targetting all allies or an enemy targetting all enemies
            if (castedByPlayer && (effect.targetType == TargettingType.AllAlliesNotSelf || effect.targetType == TargettingType.AllAllies) || !castedByPlayer && effect.targetType == TargettingType.AllEnemies)
            {
                foreach (Player p in players)
                {
                    if (p != caster && p.cHealth > 0)
                        eventQueue.Insert(new ExecuteEffectEvent(effect, caster, p, true));
                }
            }
            //If it is a player targetting all enemies or an enemy targetting all allies
            else if (!castedByPlayer && (effect.targetType == TargettingType.AllAlliesNotSelf || effect.targetType == TargettingType.AllAllies) || castedByPlayer && effect.targetType == TargettingType.AllEnemies)
            {
                foreach (Enemy enemy in enemyList)
                {
                    if (enemy != caster && enemy.cHealth > 0)
                        eventQueue.Insert(new ExecuteEffectEvent(effect, caster, enemy, true));
                }
            }
            else if (effect.targetType != TargettingType.Self)
            {
                for (int x = -Mathf.FloorToInt((castedSpell.xRange - 1) / 2.0f); x <= Mathf.CeilToInt((castedSpell.xRange - 1) / 2.0f); x++)
                {
                    for (int y = -Mathf.FloorToInt((castedSpell.yRange - 1) / 2.0f); y <= Mathf.CeilToInt((castedSpell.yRange - 1) / 2.0f); y++)
                    {
                        //If a player should be targeted and a player is at that position
                        if ((effect.targetType == TargettingType.AllInRange || castedByPlayer && effect.targetType == TargettingType.Ally || !castedByPlayer && effect.targetType == TargettingType.Enemy) && PlayerAtPos(x + castedX, y + castedY) != -1)
                            eventQueue.Insert(new ExecuteEffectEvent(effect, caster, players[PlayerAtPos(x + castedX, y + castedY)], true));
                        //If an enemy should be targeted and an enemy is at that position
                        if ((effect.targetType == TargettingType.AllInRange || !castedByPlayer && effect.targetType == TargettingType.Ally || castedByPlayer && effect.targetType == TargettingType.Enemy) && EnemyAtPos(x + castedX, y + castedY) != -1)
                            eventQueue.Insert(new ExecuteEffectEvent(effect, caster, enemyList[EnemyAtPos(x + castedX, y + castedY)], true));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Confirms the spell cast, then checks for deaths
    /// </summary>
    public void ConfirmSkillCast()
    {
        skillCastConfirmMenu.SetActive(false);
        //Checks if the player is trying to move before casting
        if (selectedMoveSpot.x != -1)
            ConfirmPlayerMove();
        //Figures out what skill the player wants to cast
        Skill displaySkill = GameStorage.skillTreeList[players[selectedPlayer].skillQuickList[selectedSpell - 1].x][players[selectedPlayer].skillQuickList[selectedSpell - 1].y];
        CastSkill(displaySkill, spellCastPosition.x, spellCastPosition.y, players[selectedPlayer]);
        FinishedMovingPawn();
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
