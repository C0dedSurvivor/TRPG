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

    private Vector2Int topLeft;

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
    public List<Enemy> enemies;
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

    //What movement animation(s) is currently running
    private List<AnimBase> currentAnimations = new List<AnimBase>();

    //A queue of all of the events that need to be run
    private BattleEventQueue eventQueue = new BattleEventQueue();

    //When it is the enemy's turn, keeps track of what enemy needs to be moved
    private int movingEnemy;

    //Whether a change has been made that would affect the states of one or more tiles
    //Keeps updateTiles from being called every frame
    private bool updateTilesThisFrame = false;

    //A list of all the temporary tile effects, the tiles they affect, and the limiters on the effect
    private TemporaryTileEffectList temporaryTileEffects = new TemporaryTileEffectList();

    public BattleUI ui;
    
    public TextAnimator littleInfoSys;

    public bool IsBattling { get { return battleState != BattleState.None || eventQueue.Count > 0 || currentAnimations.Count > 0 || !littleInfoSys.Done; } }

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
        Cursor.visible = true;
        canSwap = true;
        //Sets up the opening camera animation
        eventQueue.Insert(new MovementEvent(battleCamera, 4f, new Vector3(topLeft.x + (xSize / 2) - 0.5f, 25, topLeft.y + (ySize / 2) - 1.5f), true));
        eventQueue.Insert(new MovementEvent(battleCamera, 4f, battleCamera.transform.rotation, true));
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
        //Generates enemies
        enemies = new List<Enemy>();
        enemies.Add(new Enemy("Enemy1", 5, 5, 3, 5, 5));
        enemies.Add(new Enemy("Enemy2", 10, 5, 2, 5, 5));
        enemies.Add(new Enemy("Enemy3", 12, 5, 2, 5, 5));
        enemies.Add(new Enemy("Enemy4", 14, 5, 5, 5, 5));
        foreach (Enemy e in enemies)
        {
            e.StartOfMatch();
            CheckEventTriggers(e, EffectTriggers.StartOfMatch);
            CheckEventTriggers(e, EffectTriggers.StartOfTurn);
            participantModels.Add(e, Instantiate(EnemyBattleModelPrefab));
            participantModels[e].transform.position = new Vector3(e.position.x + topLeft.x, 1, (mapSizeY - 1) - e.position.y + topLeft.y);
        }
        eventQueue.Insert(new FunctionEvent(ui.StartBattle));
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
        if (skillCastConfirmMenu.activeSelf == false && littleInfoSys.Done)
        {
            if (currentAnimations.Count > 0)
            {
                for (int i = 0; i < currentAnimations.Count; i++)
                {
                    currentAnimations[i].StepAnimation();

                    //Debug.Log(currentAnimations[i].mover.transform.position + "|" + currentAnimations[i].finalPosition + "|" + currentAnimations[i].mover.transform.rotation.eulerAngles + "|" + currentAnimations[i].finalRotation.eulerAngles);
                    if (currentAnimations[i].IsDone())
                    {
                        //Debug.Log(GameStorage.Approximately(currentAnimations[i].mover.transform.position, currentAnimations[i].finalPosition) + " | " + currentAnimations[i].mover.transform.position + "|" + currentAnimations[i].finalPosition + "|" + eventQueue.Count);
                        if (battleState != BattleState.None)
                            updateTilesThisFrame = true;
                        //If it is a pawn being moved, execute the tile effects on them
                        if (participantModels.ContainsValue(currentAnimations[i].mover))
                        {
                            BattleParticipant pawn = null;
                            foreach (Player p in players)
                            {
                                if (participantModels[p] == currentAnimations[i].mover)
                                    pawn = p;
                            }
                            foreach (Enemy e in enemies)
                            {
                                if (participantModels[e] == currentAnimations[i].mover)
                                    pawn = e;
                            }
                            Vector3 diff = ((FlatSpeedMovementAnim)currentAnimations[i]).Difference;
                            pawn.position -= new Vector2Int(Mathf.RoundToInt(diff.x), -Mathf.RoundToInt(diff.z));
                            
                            //Triggers the tile effects that should activate from this movement
                            TriggerTileEffects(pawn, eventQueue.StillMoving(currentAnimations[i].mover) ? MoveTriggers.PassOver : MoveTriggers.StopOnTile);
                        }
                        currentAnimations[i].FinalizeAnim();
                        currentAnimations.RemoveAt(i);
                        i--;
                    }
                }
            }
            else if (eventQueue.Count != 0)
            {
                BattleEventBase currentEvent = eventQueue.GetNext();
                if (currentEvent is ExecuteEffectEvent)
                {
                    Debug.Log("EXECUTING AN EFFECT EVENT");
                    ExecuteEffectEvent trueEvent = (ExecuteEffectEvent)currentEvent;
                    if (trueEvent.caster.cHealth > 0 || trueEvent.valueFromPrevious > -1)
                        ExecuteEffect(trueEvent.effect, trueEvent.caster, trueEvent.target, trueEvent.fromSpell, trueEvent.valueFromPrevious);
                }
                if (currentEvent is MovementEvent)
                {
                    Debug.Log("EXECUTING A MOVEMENT EVENT");
                    currentAnimations.Add(((MovementEvent)currentEvent).animation);
                    if (((MovementEvent)currentEvent).animation.concurrent)
                    {
                        while (eventQueue.NextIsConcurrent())
                        {
                            currentAnimations.Add(((MovementEvent)eventQueue.GetNext()).animation);
                        }
                    }
                }
                if (currentEvent is TextEvent)
                {
                    Debug.Log("EXECUTING A TEXT EVENT");
                    littleInfoSys.Enqueue(currentEvent as TextEvent);
                }
                if (currentEvent is TurnEvent)
                {
                    TurnEvent trueEvent = (TurnEvent)currentEvent;
                    if (trueEvent.turner.cHealth > 0)
                    {
                        Debug.Log("EXECUTING A TURN EVENT");
                        trueEvent.turner.facing = trueEvent.direction;
                        participantModels[trueEvent.turner].transform.rotation = Quaternion.Euler(0, 90 * (int)trueEvent.direction, 0);
                        Debug.Log("Turned to face: " + 90 * (int)trueEvent.direction);
                    }
                }
                if (currentEvent is FunctionEvent)
                {
                    Debug.Log("EXECUTING A FUNCTION EVENT");
                    ((FunctionEvent)currentEvent).function();
                }
                if (currentEvent is FunctionEvent<BattleParticipant, BattleParticipant>)
                {
                    Debug.Log("EXECUTING A FUNCTION EVENT<T1, T2>");
                    FunctionEvent<BattleParticipant, BattleParticipant> trueEvent = (FunctionEvent<BattleParticipant, BattleParticipant>)currentEvent;
                    trueEvent.function(trueEvent.first, trueEvent.second);
                }
            }
            else
            {
                switch (battleState)
                {
                    case BattleState.None:
                        break;
                    //Moves all the enemies one at a time until there are no more left to move
                    case BattleState.Enemy:
                        if (movingEnemy >= enemies.Count)
                            eventQueue.Insert(new FunctionEvent(delegate { EndEnemyTurn(); }));
                        else
                        {
                            if (enemies[movingEnemy].CanMove())
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
                        if (n != -1)
                        {
                            players[n].position = players[selectedPlayer].position;
                            players[selectedPlayer].position = pos;
                            participantModels[players[n]].transform.position = new Vector3(players[n].position.x + topLeft.x, 1, (mapSizeY - 1) - players[n].position.y + topLeft.y);
                            participantModels[players[selectedPlayer]].transform.position = new Vector3(players[selectedPlayer].position.x + topLeft.x, 1, (mapSizeY - 1) - players[selectedPlayer].position.y + topLeft.y);
                            actionTaken = true;
                        }
                        selectedPlayer = -1;
                        updateTilesThisFrame = true;
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

                if (battleState != BattleState.Attack || tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerAttackRange) {
                    //Selecting an enemy
                    selectedEnemy = EnemyAtPos(pos.x, pos.y);
                    //If actually targetting another player for a healing attack
                    if (selectedEnemy == -1 && tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().playerAttackRange && battleState == BattleState.Attack && selectedSpell == -1)
                        selectedEnemy = enemies.Count + PlayerAtPos(pos.x, pos.y);
                }
            }
            ui.UpdateSelectedUnit();
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
            eventQueue.Insert(new MovementEvent(mapPlayer.GetComponentInChildren<Camera>().gameObject, 4f, mapPlayer.GetComponentInChildren<Camera>().transform.position, true));
            eventQueue.Insert(new MovementEvent(mapPlayer.GetComponentInChildren<Camera>().gameObject, 4f, mapPlayer.GetComponentInChildren<Camera>().transform.rotation, true));
            mapPlayer.GetComponentInChildren<Camera>().transform.SetPositionAndRotation(battleCamera.transform.position, battleCamera.transform.rotation);
            ExpungeAll();
        }
        else
        {
            battleState = BattleState.None;
            ExpungeAll();
        }
        ui.EndOfBattle();
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
            ui.UpdateSelectedUnit();
        }
    }

    /// <summary>
    /// Toggles whether enemy ranges are shown or not
    /// </summary>/
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
        updateTilesThisFrame = true;
    }

    private void ToAttack()
    {
        battleState = BattleState.Attack;
        updateTilesThisFrame = true;
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
            Debug.Log(diff);
            if (CanMoveYFirst(players[selectedPlayer], diff))
            {
                if (diff.y != 0)
                {
                    FacingDirection direction = diff.y > 0 ? FacingDirection.North : FacingDirection.South;
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], direction));
                    for (int y = 0; y < Mathf.Abs(diff.y); y++)
                    {
                        Debug.Log("Adding y");
                        eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 4f, participantModels[players[selectedPlayer]].transform.position + new Vector3(0, 0, y * Mathf.Sign(diff.y)), participantModels[players[selectedPlayer]].transform.position + new Vector3(0, 0, (y + 1) * Mathf.Sign(diff.y))));
                    }
                }

                if (diff.x != 0)
                {
                    FacingDirection direction2 = diff.x > 0 ? FacingDirection.East : FacingDirection.West;
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], direction2));
                    for (int x = 0; x < Mathf.Abs(diff.x); x++)
                    {
                        Debug.Log("Adding x");
                        eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 4f, participantModels[players[selectedPlayer]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0, diff.y), participantModels[players[selectedPlayer]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0, diff.y)));
                    }
                }
            }
            else
            {
                if (diff.x != 0)
                {
                    FacingDirection direction = diff.x > 0 ? FacingDirection.East : FacingDirection.West;
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], direction));
                    for (int x = 0; x < Mathf.Abs(diff.x); x++)
                    {
                        eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 4f, participantModels[players[selectedPlayer]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0), participantModels[players[selectedPlayer]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0)));
                    }
                }
                if (diff.y != 0)
                {
                    FacingDirection direction2 = diff.y > 0 ? FacingDirection.North : FacingDirection.South;
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], direction2));
                    for (int y = 0; y < Mathf.Abs(diff.y); y++)
                    {
                        eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], 4f, participantModels[players[selectedPlayer]].transform.position + new Vector3(diff.x, 0, y * Mathf.Sign(diff.y)), participantModels[players[selectedPlayer]].transform.position + new Vector3(diff.x, 0, (y + 1) * Mathf.Sign(diff.y))));
                    }
                }
            }
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
        if (selectedEnemy >= enemies.Count)
            eventQueue.Insert(new ExecuteEffectEvent(new HealingPart(TargettingType.Ally, players[selectedPlayer].GetEffectiveStat(Stats.MagicAttack) / 2, 0, 0), players[selectedPlayer], players[selectedEnemy - enemies.Count]));
        //If attacking an enemy
        else
            PerformAttack(players[selectedPlayer], enemies[selectedEnemy]);
        FinishedMovingPawn();
    }

    /// <summary>
    /// Sets up for the next pawn to be moved
    /// or
    /// If all player pawns have finished moving, set up for enemies to move
    /// </summary>
    public void FinishedMovingPawn()
    {
        if (selectedPlayer != -1)
        {
            players[selectedPlayer].moved = true;
            selectedMoveSpot = new Vector2Int(-1, -1);
        }
        selectedPlayer = -1;
        selectedEnemy = -1;
        selectedSpell = -1;
        updateTilesThisFrame = true;
        battleState = BattleState.Player;

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
            for (int j = 0; j < enemies.Count; j++)
            {
                enemies[j].moved = !enemies[j].CanMove();
            }
            battleState = BattleState.Enemy;
        }
        ui.UpdateSelectedUnit();
    }

    /// <summary>
    /// If player wants to end the turn before all ally pawns have been moved
    /// </summary>
    public void EndPlayerTurnEarly()
    {
        if (eventQueue.Count == 0)
        {
            moveMarker.SetActive(false);
            canSwap = false;
            foreach (Player p in players)
            {
                p.moved = true;
            }
            FinishedMovingPawn();
        }
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
        List<Vector2Int> moveSpots = GetViableMovements(enemies[ID]);
        WeaponType weapon = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[enemies[ID].equippedWeapon.Name]).subType];
        foreach (Vector2Int pos in moveSpots)
        {
            List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, new Vector2Int(pos.x, pos.y));
            foreach (Vector2Int aPos in attackSpots)
            {
                if (PlayerAtPos(aPos.x, aPos.y) != -1)
                {
                    possibleMoves.Add(new EnemyMove(pos.x, pos.y, aPos.x, aPos.y, 15 - (Mathf.Abs(pos.x - enemies[ID].position.x) + Mathf.Abs(pos.y - enemies[ID].position.y)), 1));
                }
            }
            foreach (Player p in players)
            {
                //If this move is the closest the enemy can get to a player, make it the move that happens if no attacks are possible
                if (Vector2Int.Distance(p.position, pos) < fallbackMove.priority)
                {
                    fallbackMove = new EnemyMove(pos.x, pos.y, Vector2Int.Distance(p.position, pos), 0);
                }
                //If this move ties with the current fallback move for distance from a player, pick a random one
                else if (Vector2Int.Distance(p.position, pos) == fallbackMove.priority)
                {
                    if (Random.Range(0, 2) == 1)
                        fallbackMove = new EnemyMove(pos.x, pos.y, Vector2Int.Distance(p.position, pos), 0);
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
        Vector2Int diff = possibleMoves[0].movePosition - enemies[ID].position;
        diff.y = -diff.y;

        if (CanMoveYFirst(enemies[ID], diff))
        {
            if (diff.y != 0)
            {
                FacingDirection direction = diff.y > 0 ? FacingDirection.North : FacingDirection.South;
                Debug.Log("Inserting turn " + direction);
                eventQueue.Insert(new TurnEvent(enemies[ID], direction));
                for (int y = 0; y < Mathf.Abs(diff.y); y++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[enemies[ID]], 4f, participantModels[enemies[ID]].transform.position + new Vector3(0, 0, y * Mathf.Sign(diff.y)), participantModels[enemies[ID]].transform.position + new Vector3(0, 0, (y + 1) * Mathf.Sign(diff.y))));
                }
            }
            if (diff.x != 0)
            {
                FacingDirection direction2 = diff.x > 0 ? FacingDirection.East : FacingDirection.West;
                Debug.Log("Inserting turn " + direction2);
                eventQueue.Insert(new TurnEvent(enemies[ID], direction2));
                for (int x = 0; x < Mathf.Abs(diff.x); x++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[enemies[ID]], 4f, participantModels[enemies[ID]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0, diff.y), participantModels[enemies[ID]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0, diff.y)));
                }
            }
        }
        else
        {
            if (diff.x != 0)
            {
                FacingDirection direction = diff.x > 0 ? FacingDirection.East : FacingDirection.West;
                Debug.Log("OOPS");
                eventQueue.Insert(new TurnEvent(enemies[ID], direction));
                for (int x = 0; x < Mathf.Abs(diff.x); x++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[enemies[ID]], 4f, participantModels[enemies[ID]].transform.position + new Vector3(x * Mathf.Sign(diff.x), 0), participantModels[enemies[ID]].transform.position + new Vector3((x + 1) * Mathf.Sign(diff.x), 0)));
                }
            }
            if (diff.y != 0)
            {
                FacingDirection direction2 = diff.y > 0 ? FacingDirection.North : FacingDirection.South;
                Debug.Log("OOPS");
                eventQueue.Insert(new TurnEvent(enemies[ID], direction2));
                for (int y = 0; y < Mathf.Abs(diff.y); y++)
                {
                    eventQueue.Insert(new MovementEvent(participantModels[enemies[ID]], 4f, participantModels[enemies[ID]].transform.position + new Vector3(diff.x, 0, y * Mathf.Sign(diff.y)), participantModels[enemies[ID]].transform.position + new Vector3(diff.x, 0, (y + 1) * Mathf.Sign(diff.y))));
                }
            }
        }
        enemies[ID].moved = true;
        if (possibleMoves[0].attackPosition.x != -1)
            eventQueue.Insert(new FunctionEvent<BattleParticipant, BattleParticipant>(PerformAttack, enemies[movingEnemy], players[PlayerAtPos(possibleMoves[0].attackPosition.x, possibleMoves[0].attackPosition.y)]));
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

                //Checks if any tile effects for ending a turn on a tile need to be done
                TriggerTileEffects(p, MoveTriggers.EndOfTurn);

                CheckEventTriggers(p, EffectTriggers.EndOfTurn);
            }
        }
        foreach (Enemy e in enemies)
        {
            if (e.cHealth > 0)
            {
                e.EndOfTurn();

                //Checks if any tile effects for ending a turn on a tile need to be done
                TriggerTileEffects(e, MoveTriggers.EndOfTurn);

                CheckEventTriggers(e, EffectTriggers.EndOfTurn);
            }
        }
        temporaryTileEffects.EndOfTurn();
        foreach (Player p in players)
        {
            if (p.cHealth > 0)
            {
                p.StartOfTurn();

                //Checks if any tile effects for ending a turn on a tile need to be done
                TriggerTileEffects(p, MoveTriggers.StartOfTurn);

                CheckEventTriggers(p, EffectTriggers.StartOfTurn);
                p.moved = !p.CanMove();
            }
        }
        foreach (Enemy e in enemies)
        {
            if (e.cHealth > 0)
            {
                e.StartOfTurn();

                //Checks if any tile effects for ending a turn on a tile need to be done
                TriggerTileEffects(e, MoveTriggers.StartOfTurn);

                CheckEventTriggers(e, EffectTriggers.StartOfTurn);
            }
        }
        temporaryTileEffects.StartOfTurn();
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
        foreach (Enemy e in enemies)
        {
            if (e.cHealth <= 0)
            {
                deadCount++;
                participantModels[e].SetActive(false);
                e.position.Set(-200, -200);
            }
        }
        if (deadCount == enemies.Count)
            OnBattleEnd(true);
    }

    /// <summary>
    /// Gets how much damage attacker would do when attacking target
    /// </summary>
    /// <param name="attacker">The pawn doing the attacking</param>
    /// <param name="target">The pawn getting attacked</param>
    /// <returns>The first value is the amount of damage, the second is the damage type and whether or not the weapon is ranged</returns>
    public Pair<int, DamageType> GetDamageValues(BattleParticipant attacker, BattleParticipant target)
    {
        //Gets the distance between the player and enemy
        Vector2Int dist = target.position - attacker.position;

        WeaponStatsAtRange stats = attacker.GetWeaponStatsAtDistance(dist);

        //Checks for a critical hit, which multiplies damage by 1.5 and checks the basic attack modifiers on the pawns
        float mod = stats.damageMult * (Random.Range(0, 100) < attacker.GetEffectiveStat(Stats.CritChance) ? 1.5f : 1.0f) *
            (attacker.GetEffectiveStat(Stats.BasicAttackEffectiveness) / 100.0f) * (target.GetEffectiveStat(Stats.BasicAttackReceptiveness) / 100.0f);

        if (stats.damageType == DamageType.Physical)
            return new Pair<int, DamageType>(Mathf.RoundToInt((attacker.GetEffectiveStat(Stats.Attack) * 3.0f * mod) / target.GetEffectiveStat(Stats.Defense)), stats.damageType);
        else
            return new Pair<int, DamageType>(Mathf.RoundToInt((attacker.GetEffectiveStat(Stats.MagicAttack) * mod * 3.0f) / target.GetEffectiveStat(Stats.MagicDefense)), stats.damageType);
    }

    /// <summary>
    /// Performs attack, then checks for an executes possible counterattack if both pawns are still alive
    /// </summary>
    /// <param name="attacker">The pawn that is initially attacking</param>
    /// <param name="defender">The pawn that is initially defending</param>
    public void PerformAttack(BattleParticipant attacker, BattleParticipant defender)
    {
        Pair<int, DamageType> attackData = GetDamageValues(attacker, defender);

        eventQueue.Insert(new TextEvent(attacker.name + " attacks " + defender.name + "!"));
        int damage = defender.GetDamage(attackData.First);
        CheckEventTriggers(attacker, EffectTriggers.BasicAttack, defender, damage);
        CheckEventTriggers(defender, EffectTriggers.HitWithBasicAttack, attacker, damage);

        eventQueue.Insert(new ExecuteEffectEvent(new DamagePart(TargettingType.Enemy, attackData.Second, 0, attackData.First, 0, 0), attacker, defender));

        //Lifesteal on basic attack hit
        if (damage > 0 && attacker.GetEffectiveStat(Stats.BasicAttackLifesteal) > 0)
        {
            eventQueue.Insert(new TextEvent(attacker.name + " lifesteals from the blow."));
            eventQueue.Insert(new ExecuteEffectEvent(new HealingPart(TargettingType.Self, 0, Mathf.CeilToInt(damage * attacker.GetEffectiveStat(Stats.BasicAttackLifesteal) / 100.0f), 0), attacker, attacker));
        }

        eventQueue.Insert(new FunctionEvent<BattleParticipant, BattleParticipant>(PerformCounterattack, defender, attacker));
    }

    /// <summary>
    /// Performs a counterattack if one can be performed
    /// </summary>
    /// <param name="attacker">The counterattacker, aka the one who was hit by the initial attack</param>
    /// <param name="defender">The former attacker</param>
    public void PerformCounterattack(BattleParticipant attacker, BattleParticipant defender)
    {
        //If the defender lives and can attack back at that range
        if (attacker.cHealth > 0 && defender.cHealth > 0 && attacker.GetWeaponStatsAtDistance(attacker.position - defender.position) != null)
        {
            eventQueue.Insert(new TextEvent(attacker.name + " returns the favor!"));
            Pair<int, DamageType> attackData = GetDamageValues(attacker, defender);
            int damage = defender.GetDamage(Mathf.RoundToInt(attackData.First));
            CheckEventTriggers(attacker, EffectTriggers.BasicAttack, defender, damage);
            CheckEventTriggers(defender, EffectTriggers.HitWithBasicAttack, attacker, damage);

            eventQueue.Insert(new ExecuteEffectEvent(new DamagePart(TargettingType.Enemy, attackData.Second, 0, attackData.First, 0, 0), attacker, defender));

            //Lifesteal on basic attack hit
            if (damage > 0 && attacker.GetEffectiveStat(Stats.BasicAttackLifesteal) > 0)
            {
                eventQueue.Insert(new TextEvent(attacker.name + " lifesteals from the blow."));
                eventQueue.Insert(new ExecuteEffectEvent(new HealingPart(TargettingType.Self, 0, Mathf.CeilToInt(damage * attacker.GetEffectiveStat(Stats.BasicAttackLifesteal) / 100.0f), 0), attacker, attacker));
            }

            eventQueue.Insert(new FunctionEvent< BattleParticipant, BattleParticipant > (CheckForKill, attacker, defender));
        }
    }

    /// <summary>
    /// Checks to see if a pawn dies from a source of damage and checks for relevant triggers
    /// </summary>
    /// <param name="attacker">The pawn that may have killed another</param>
    /// <param name="defender">The target that may have died</param>
    private void CheckForKill(BattleParticipant attacker, BattleParticipant defender)
    {
        if (defender.cHealth <= 0)
        {
            CheckEventTriggers(defender, EffectTriggers.Die);
            CheckEventTriggers(attacker, EffectTriggers.KillAnEnemy);
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
        for (int e = 0; e < enemies.Count; e++)
        {
            if (enemies[e].position.x == x && enemies[e].position.y == y)
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
                tileList[p.position.x, (mapSizeY - 1) - p.position.y].GetComponent<BattleTile>().enemyDanger = true;
            }
            if(selectedPlayer != -1)
                tileList[players[selectedPlayer].position.x, (mapSizeY - 1) - players[selectedPlayer].position.y].GetComponent<BattleTile>().enemyDanger = false;
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
                //If the player can attack an enemy or heal an ally at that position
                if (EnemyAtPos(attackPos.x, attackPos.y) != -1 || (PlayerAtPos(attackPos.x, attackPos.y) != -1 && weapon.GetStatsAtRange(attackPos - players[selectedPlayer].position).heals))
                    tileList[attackPos.x, (mapSizeY - 1) - attackPos.y].GetComponent<BattleTile>().playerAttackRange = true;
            }
        }

        //Renders the possible movements for a given player-controlled pawn
        else if (battleState == BattleState.Player)
        {
            if (selectedPlayer != -1)
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
            //If there is a selected enemy but no selected player, render the enemy's ranges
            else if(selectedEnemy != -1)
            {
                List<Vector2Int> moveSpots = GetViableMovements(enemies[selectedEnemy]);
                foreach (Vector2Int pos in moveSpots)
                {
                    tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().enemyDanger = true;
                    List<Vector2Int> attackSpots = GetViableAttackSpaces(Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[enemies[selectedEnemy].equippedWeapon.Name]).subType], pos);
                    foreach (Vector2Int attackPos in attackSpots)
                    {
                        tileList[attackPos.x, (mapSizeY - 1) - attackPos.y].GetComponent<BattleTile>().enemyDanger = true;
                    }
                }
            }
        }

        //Renders the movement and attack ranges of enemies if requested
        if (showDanger)
        {
            foreach (Enemy e in enemies)
            {
                List<Vector2Int> moveSpots = GetViableMovements(e);
                foreach (Vector2Int pos in moveSpots)
                {
                    tileList[pos.x, (mapSizeY - 1) - pos.y].GetComponent<BattleTile>().enemyDanger = true;
                    List<Vector2Int> attackSpots = GetViableAttackSpaces(Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[e.equippedWeapon.Name]).subType], pos);
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
        updateTilesThisFrame = false;
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
    /// Returns whether or not the path a pawn can take to a given position can be horizontal first
    /// </summary>
    /// <param name="mover">The pawn that is moving</param>
    /// <param name="relativeMove">The position they want to move to relative to their current position</param>
    private bool CanMoveXFirst(BattleParticipant mover, Vector2Int relativeMove)
    {
        for (int x = 0; x <= Mathf.Abs(relativeMove.x); x++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.position.y]))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.position.y) : PlayerAtPos(mover.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.position.y)) != -1)
                return false;
        }

        for (int y = 0; y <= Mathf.Abs(relativeMove.y); y++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.position.x + relativeMove.x, mover.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))]))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.position.x + relativeMove.x, mover.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))) : PlayerAtPos(mover.position.x + relativeMove.x, mover.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y)))) != -1)
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
        int maxMove = entity.GetEffectiveStat(Stats.MaxMove);
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

                    if (CanMoveYFirst(entity, new Vector2Int(x, y)) || CanMoveXFirst(entity, new Vector2Int(x, y)))
                        moveSpots.Add(new Vector2Int(x + entity.position.x, y + entity.position.y));
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
        //Gets all of the straight viable attack position
        foreach (WeaponStatsAtRange statsAtRange in weapon.ranges)
        {
            if (!statsAtRange.ranged && statsAtRange.atDistance > 1)
            {
                if (center.x + statsAtRange.atDistance < mapSizeX && battleMap[center.x + statsAtRange.atDistance, (mapSizeY - 1) - center.y] != (int)BattleTiles.Impassable && attackSpots.Contains(new Vector2Int(center.x + statsAtRange.atDistance - 1, center.y)))
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, center.y));

                if (center.x - statsAtRange.atDistance >= 0 && battleMap[center.x - statsAtRange.atDistance, (mapSizeY - 1) - center.y] != (int)BattleTiles.Impassable && attackSpots.Contains(new Vector2Int(center.x - statsAtRange.atDistance + 1, center.y)))
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, center.y));

                if (center.y + statsAtRange.atDistance < mapSizeY && battleMap[center.x, (mapSizeY - 1) - (center.y + statsAtRange.atDistance)] != (int)BattleTiles.Impassable && attackSpots.Contains(new Vector2Int(center.x, center.y + statsAtRange.atDistance - 1)))
                    attackSpots.Add(new Vector2Int(center.x, center.y + statsAtRange.atDistance));

                if (center.y - statsAtRange.atDistance >= 0 && battleMap[center.x, (mapSizeY - 1) - (center.y - statsAtRange.atDistance)] != (int)BattleTiles.Impassable && attackSpots.Contains(new Vector2Int(center.x, center.y - statsAtRange.atDistance + 1)))
                    attackSpots.Add(new Vector2Int(center.x, center.y - statsAtRange.atDistance));
            }
            //If the weapon is ranged or the space is right against the attacker it just needs to be a place entities can exist
            else
            {
                if (center.x + statsAtRange.atDistance < mapSizeX && battleMap[center.x + statsAtRange.atDistance, (mapSizeY - 1) - center.y] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, center.y));
                if (center.x - statsAtRange.atDistance >= 0 && battleMap[center.x - statsAtRange.atDistance, (mapSizeY - 1) - center.y] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, center.y));
                if (center.y + statsAtRange.atDistance < mapSizeY && battleMap[center.x, (mapSizeY - 1) - (center.y + statsAtRange.atDistance)] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x, center.y + statsAtRange.atDistance));
                if (center.y - statsAtRange.atDistance >= 0 && battleMap[center.x, (mapSizeY - 1) - (center.y - statsAtRange.atDistance)] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x, center.y - statsAtRange.atDistance));
            }
        }
        //Gets all of the diagonal viable attack position
        foreach (WeaponStatsAtRange statsAtRange in weapon.diagonalRanges)
        {
            if (center.y + statsAtRange.atDistance < mapSizeY)
            {
                if (center.x - statsAtRange.atDistance >= 0 && battleMap[center.x - statsAtRange.atDistance, (mapSizeY - 1) - (center.y + statsAtRange.atDistance)] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, center.y + statsAtRange.atDistance));
                if (center.x + statsAtRange.atDistance < mapSizeX && battleMap[center.x + statsAtRange.atDistance, (mapSizeY - 1) - (center.y + statsAtRange.atDistance)] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, center.y + statsAtRange.atDistance));
            }
            if (center.y - statsAtRange.atDistance >= 0)
            {
                if (center.x - statsAtRange.atDistance >= 0 && battleMap[center.x - statsAtRange.atDistance, (mapSizeY - 1) - (center.y - statsAtRange.atDistance)] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, center.y - statsAtRange.atDistance));
                if (center.x + statsAtRange.atDistance < mapSizeX && battleMap[center.x + statsAtRange.atDistance, (mapSizeY - 1) - (center.y - statsAtRange.atDistance)] != (int)BattleTiles.Impassable)
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, center.y - statsAtRange.atDistance));
            }
        }
        return attackSpots;
    }

    /// <summary>
    /// Activates all the triggers on a pawn from the tile they're on
    /// </summary>
    /// <param name="pawn">What pawn is on this tile</param>
    /// <param name="trigger">The type of effects to trigger</param>
    private void TriggerTileEffects(BattleParticipant pawn, MoveTriggers trigger)
    {
        List<TileEffects> effects = new List<TileEffects>();
        if (Registry.DefaultTileEffects.ContainsKey(battleMap[pawn.position.x, pawn.position.y]) && Registry.DefaultTileEffects[battleMap[pawn.position.x, pawn.position.y]].Contains(trigger))
            effects.Add(Registry.DefaultTileEffects[battleMap[pawn.position.x, pawn.position.y]]);
        effects.AddRange(temporaryTileEffects.GetTileEffects(pawn.position, trigger));
        foreach (TileEffects effect in effects)
        {
            effect.effects[trigger].target = pawn;
            eventQueue.Insert(effect.effects[trigger]);
        }
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
        Debug.Log("Checking for trigger: " + trigger + ". Results: " + list.Count);
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
                foreach (Enemy enemy in enemies)
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
        if (target != null && Random.Range(0, 101) <= effect.chanceToProc)
        {
            //Flat damage, then calculated damage, then remaining hp, then max hp
            if (effect is DamagePart)
            {
                DamagePart trueEffect = effect as DamagePart;
                int previousHealth = target.cHealth;
                //If this is supposed to be modified by a previous value, do so.
                float mod = trueEffect.modifiedByValue != 0 ? trueEffect.modifiedByValue * valueFromPrevious : 1;
                //Takes into account the appropriate player stats
                if (fromSpell)
                {
                    mod *= (caster.GetEffectiveStat(Stats.SpellDamageEffectiveness) / 100.0f) * (target.GetEffectiveStat(Stats.SpellDamageReceptiveness) / 100.0f);
                }
                int damage = target.Damage(Mathf.RoundToInt(trueEffect.flatDamage * mod));
                damage += target.Damage(Mathf.RoundToInt((trueEffect.damage * mod *
                    (trueEffect.damageType == DamageType.Physical ? caster.GetEffectiveStat(Stats.Attack) : caster.GetEffectiveStat(Stats.MagicAttack)) * 3.0f) /
                    (trueEffect.damageType == DamageType.Physical ? caster.GetEffectiveStat(Stats.Defense) : caster.GetEffectiveStat(Stats.MagicDefense))));
                damage += target.Damage((int)(target.cHealth * trueEffect.remainingHpPercent * mod / 100.0f));
                damage += target.Damage((int)(target.GetEffectiveStat(Stats.MaxHealth) * trueEffect.maxHpPercent * mod / 100.0f));
                eventQueue.Insert(new TextEvent(target.name + " takes " + damage + " damage!"));
                //Keeps effects from stacking on top of each other by a looped damage call
                if (trueEffect.modifiedByValue == 0 && damage > 0)
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
                    if (fromSpell && caster.GetEffectiveStat(Stats.SpellLifesteal) > 0)
                    {
                        eventQueue.Insert(new TextEvent(caster.name + " steals some life essence from the spell."));
                        eventQueue.Insert(new ExecuteEffectEvent(new HealingPart(TargettingType.Self, 0, Mathf.CeilToInt(damage * caster.GetEffectiveStat(Stats.SpellLifesteal) / 100.0f), 0), caster, caster));
                    }
                }

                //If this causes them to fall below 50% health
                if (previousHealth >= target.GetEffectiveStat(Stats.MaxHealth) / 2.0 && target.cHealth < target.GetEffectiveStat(Stats.MaxHealth) / 2.0)
                    CheckEventTriggers(target, EffectTriggers.FallBelow50Percent, caster);
                //If this causes them to fall below 50% health
                if (previousHealth >= target.GetEffectiveStat(Stats.MaxHealth) / 4.0 && target.cHealth < target.GetEffectiveStat(Stats.MaxHealth) / 4.0)
                    CheckEventTriggers(target, EffectTriggers.FallBelow25Percent, caster);
                //If the target dies
                eventQueue.Insert(new FunctionEvent<BattleParticipant, BattleParticipant>(CheckForKill, caster, target));

                eventQueue.Insert(new FunctionEvent(delegate { CheckForDeath(); }));
            }

            //Flat healing, then calculated healing, then max hp
            else if (effect is HealingPart)
            {
                HealingPart trueEffect = effect as HealingPart;
                //If this is supposed to be modified by a previous value, do so. Otherwise, don't
                float mod = (trueEffect.modifiedByValue != 0 ? trueEffect.modifiedByValue * valueFromPrevious : 1)
                    * (caster != null ? (caster.GetEffectiveStat(Stats.HealingEffectiveness) / 100.0f) : 1) * (target.GetEffectiveStat(Stats.HealingReceptiveness) / 100.0f);

                int healing = target.Heal(Mathf.RoundToInt(trueEffect.flatHealing * mod));
                healing += target.Heal(Mathf.RoundToInt((trueEffect.healing * mod * caster.GetEffectiveStat(Stats.MagicAttack) * 3.0f) / target.GetEffectiveStat(Stats.Defense)));
                healing += target.Heal((int)(target.GetEffectiveStat(Stats.MaxHealth) * trueEffect.maxHpPercent * mod / 100.0f));
                eventQueue.Insert(new TextEvent(target.name + " heals for " + healing + " health!"));
                //Keeps effects from stacking on top of each other by a looped healing call
                if (trueEffect.modifiedByValue == 0 && healing > 0)
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
                if(trueEffect.StatMod.flatMod < 0)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.StatMod.affectedStat) + " was decreased by " + trueEffect.StatMod.flatMod + "!"));
                else if(trueEffect.StatMod.flatMod > 0)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.StatMod.affectedStat) + " was increased by " + trueEffect.StatMod.flatMod + "!"));
                if (trueEffect.StatMod.multMod < 1)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.StatMod.affectedStat) + " was decreased by " + ((1 - trueEffect.StatMod.flatMod) * 100) + "%!"));
                else if (trueEffect.StatMod.multMod > 1)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.StatMod.affectedStat) + " was increased by " + ((trueEffect.StatMod.flatMod - 1) * 100) + "%!"));
                target.AddMod(trueEffect.StatMod);
            }

            //Adds or removes status effects
            else if (effect is StatusEffectPart)
            {
                StatusEffectPart trueEffect = effect as StatusEffectPart;
                if (trueEffect.remove)
                {
                    eventQueue.Insert(new TextEvent(target.name + " is cleansed of " + trueEffect.status + "!"));
                    target.RemoveStatusEffect(trueEffect.status);
                }
                else
                {
                    eventQueue.Insert(new TextEvent(target.name + " in afflicted with " + trueEffect.status + "!"));
                    target.AddStatusEffect(trueEffect.status);
                }
            }

            //Adds a temporary trigger to the target
            else if (effect is AddTriggerPart)
            {
                AddTriggerPart trueEffect = effect as AddTriggerPart;
                target.AddTemporaryTrigger(trueEffect.effect, trueEffect.maxTimesThisBattle, trueEffect.turnCooldown, trueEffect.maxActiveTurns);
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
                            eventQueue.Insert(new MovementEvent(participantModels[target], 0.1f, participantModels[target].transform.position + new Vector3Int(0, 0, i - 1) * dir, participantModels[target].transform.position + new Vector3Int(0, 0, i) * dir));
                        else
                        {
                            //If the pawn hits a spot where they can't move any further, stun them for a turn
                            eventQueue.Insert(new ExecuteEffectEvent(new StatusEffectPart(TargettingType.AllInRange, Statuses.Stun, false), caster, target));
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
                            eventQueue.Insert(new MovementEvent(participantModels[target], 0.1f, participantModels[target].transform.position + new Vector3Int(i - 1, 0, 0) * dir, participantModels[target].transform.position + new Vector3Int(i, 0, 0) * dir));
                        else
                        {
                            //If the pawn hits a spot where they can't move any further, stun them for a turn
                            eventQueue.Insert(new ExecuteEffectEvent(new StatusEffectPart(TargettingType.AllInRange, Statuses.Stun, false), caster, target));
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
                        if (target.moved)
                        {
                            //Check if the target is able to move again
                            bool canMove = target.CanMove();
                            target.moved = !canMove;
                            if (canMove)
                            {
                                eventQueue.Insert(new TextEvent(target.name + " is reinvigorated and can move again!"));
                                //If it is an enemy that can move again, moves back through the enemy list and tries to move them again
                                if (battleState == BattleState.Enemy)
                                {
                                    movingEnemy = 0;
                                }
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

        eventQueue.Insert(new TextEvent(caster.name + " casts " + castedSpell.name + "."));

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
                foreach (Enemy enemy in enemies)
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
                            eventQueue.Insert(new ExecuteEffectEvent(effect, caster, enemies[EnemyAtPos(x + castedX, y + castedY)], true));
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
