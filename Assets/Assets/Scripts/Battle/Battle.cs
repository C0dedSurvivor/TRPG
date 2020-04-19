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
    public GameObject CameraPrefab;

    public GameObject skillCastConfirmMenu;

    public Vector2Int bottomLeft;

    //Battle state for the finite state machine
    public static BattleState battleState = BattleState.None;
    //Whether or not the players can swap positions, only true if no one has moved yet
    public bool canSwap;

    //Affects what the enemies take into account when making their moves, see MoveEnemies() for more information
    int difficulty;

    //Whether a change has been made that would affect the states of one or more tiles
    //Keeps updateTiles from being called every frame
    public bool updateTilesThisFrame = false;
    public BattleMap graphicalBattleMap;

    //Stores the data representation of the current chunk of the world, dictates where participants can move
    public BattleTile[,] battleMap;
    //Stores the aEther levels of the area, slot 0 = current level, slot 1 = max level
    public int[,,] aEtherMap;
    //Stores the players used in this battle
    public List<Player> players;
    //Stores the enemy data
    public List<Enemy> enemies;
    //Stores the visual representation of the participants
    public Dictionary<BattlePawnBase, GameObject> participantModels = new Dictionary<BattlePawnBase, GameObject>();
    //This is a camera
    private GameObject battleCamera;
    public GameObject mapPlayer;
    public float pawnMoveSpeed = 4.0f;

    //-1 means nothing selected
    public int selectedPlayer = -1;
    public int selectedEnemy = -1;
    public int hoveredSpell = -1;
    public int selectedSpell = -1;
    private int turn = 1;
    public Vector2Int selectedMoveSpot = new Vector2Int(-1, -1);
    private Vector2Int spellCastPosition = new Vector2Int(-1, -1);
    //If the current spell cast position is on a valid target
    public bool skillLegitTarget = false;

    //What movement animation(s) is currently running
    private List<AnimBase> currentAnimations = new List<AnimBase>();

    //A queue of all of the events that need to be run
    private BattleEventQueue eventQueue = new BattleEventQueue();

    //When it is the enemy's turn, keeps track of what enemy needs to be moved
    private int movingEnemy;

    //A list of all the temporary tile effects, the tiles they affect, and the limiters on the effect
    private TemporaryTileEffectList temporaryTileType = new TemporaryTileEffectList();

    public BattleUI ui;

    public TextAnimator littleInfoSys;

    public bool IsBattling { get { return battleState != BattleState.None || eventQueue.Count > 0 || currentAnimations.Count > 0 || !littleInfoSys.Done; } }

    #endregion

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
        //Grabs the map layout
        battleMap = new BattleTile[BattleMap.mapSizeX, BattleMap.mapSizeY];
        int[,] typeMap = GameStorage.GrabBattleMap(centerX, centerY, xSize, ySize);
        for (int x = 0; x < BattleMap.mapSizeX; x++)
        {
            for (int y = 0; y < BattleMap.mapSizeY; y++)
            {
                battleMap[x, y] = new BattleTile(typeMap[x, y]);
            }
        }
        //Finds the top left corner of the current map
        bottomLeft = new Vector2Int(GameStorage.trueBX, GameStorage.trueBY);
        //Generates the visible tile map
        graphicalBattleMap.StartOfBattle(bottomLeft.x, bottomLeft.y);
        //Grabs the aEther map
        aEtherMap = GameStorage.GrabaEtherMap(bottomLeft.x, bottomLeft.y, xSize, ySize);
        //Make the camera
        battleCamera = Instantiate(CameraPrefab);
        skillCastConfirmMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        canSwap = true;
        //Sets up the opening camera animation
        eventQueue.Insert(new MovementEvent(battleCamera, 4f, new Vector3(bottomLeft.x + (xSize / 2) - 0.5f, 25, bottomLeft.y + (ySize / 2) - 1.5f), true));
        eventQueue.Insert(new MovementEvent(battleCamera, 4f, battleCamera.transform.rotation, true));
        battleCamera.transform.SetPositionAndRotation(mainCamera.position, mainCamera.rotation);
        eventQueue.Insert(new FunctionEvent(ToMatch));
        players = new List<Player>();
        //Moves the player and enemy models into their correct position and sets up default values
        for (int i = 0; i < GameStorage.activePlayerList.Count; i++)
        {
            players.Add(GameStorage.playerMasterList[GameStorage.activePlayerList[i]]);
            players[i].tempStats = new BattleOnlyStats(6 + 2 * i, 5 + i % 2, players[i]);
            CheckEventTriggers(players[i], EffectTriggers.StartOfMatch);
            CheckEventTriggers(players[i], EffectTriggers.StartOfTurn);
            participantModels.Add(players[i], Instantiate(PlayerBattleModelPrefab));
            participantModels[players[i]].transform.position = new Vector3(
                players[i].tempStats.position.x + bottomLeft.x,
                1 + graphicalBattleMap.GetHeightAtGlobalPos(new Vector3(players[i].tempStats.position.x + bottomLeft.x, 0, players[i].tempStats.position.y + bottomLeft.y)),
                players[i].tempStats.position.y + bottomLeft.y
                );
        }
        //Generates enemies
        enemies = new List<Enemy>();
        enemies.Add(new Enemy("Enemy1", 5, 11, 2, 5, 5));
        enemies.Add(new Enemy("Enemy2", 10, 11, 1, 5, 5));
        enemies.Add(new Enemy("Enemy3", 12, 11, 1, 5, 5));
        enemies.Add(new Enemy("Enemy4", 14, 11, 4, 5, 5));
        foreach (Enemy e in enemies)
        {
            CheckEventTriggers(e, EffectTriggers.StartOfMatch);
            CheckEventTriggers(e, EffectTriggers.StartOfTurn);
            participantModels.Add(e, Instantiate(EnemyBattleModelPrefab));
            participantModels[e].transform.position = new Vector3(
                e.tempStats.position.x + bottomLeft.x,
                1 + graphicalBattleMap.GetHeightAtGlobalPos(new Vector3(e.tempStats.position.x + bottomLeft.x, 0, e.tempStats.position.y + bottomLeft.y)),
                e.tempStats.position.y + bottomLeft.y
                );
        }
        eventQueue.Insert(new FunctionEvent(ui.StartBattle));
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

                    //Debug.Log(currentAnimations[i].mover.transform.tempStats.position + "|" + currentAnimations[i].finalPosition + "|" + currentAnimations[i].mover.transform.rotation.eulerAngles + "|" + currentAnimations[i].finalRotation.eulerAngles);
                    if (currentAnimations[i].IsDone())
                    {
                        //Debug.Log(GameStorage.Approximately(currentAnimations[i].mover.transform.tempStats.position, currentAnimations[i].finalPosition) + " | " + currentAnimations[i].mover.transform.tempStats.position + "|" + currentAnimations[i].finalPosition + "|" + eventQueue.Count);
                        if (battleState != BattleState.None)
                            updateTilesThisFrame = true;
                        //If it is a pawn being moved, execute the tile effects on them
                        if (participantModels.ContainsValue(currentAnimations[i].mover))
                        {
                            BattlePawnBase pawn = null;
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
                            Vector3 diff = ((StitchedFlatSpeedMovementAnim)currentAnimations[i]).difference;
                            pawn.tempStats.position -= new Vector2Int(Mathf.RoundToInt(diff.x), Mathf.RoundToInt(diff.z));

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
                if (currentEvent is ExecuteEffectEvent && battleState != BattleState.None)
                {
                    Debug.Log("EXECUTING AN EFFECT EVENT");
                    ExecuteEffectEvent trueEvent = (ExecuteEffectEvent)currentEvent;
                    if (trueEvent.caster.cHealth > 0 || trueEvent.valueFromPrevious > -1)
                        ExecuteEffect(trueEvent.effect, trueEvent.caster, trueEvent.target, trueEvent.fromSpell, trueEvent.valueFromPrevious);
                }
                if (currentEvent is MovementEvent)
                {
                    Debug.Log("EXECUTING A MOVEMENT EVENT");
                    MovementEvent trueEvent = (MovementEvent)currentEvent;
                    currentAnimations.Add(trueEvent.animation);
                    if (trueEvent.animation.concurrent)
                    {
                        while (eventQueue.NextIsConcurrent())
                        {
                            currentAnimations.Add(((MovementEvent)eventQueue.GetNext()).animation);
                        }
                    }
                }
                if (currentEvent is TextEvent && battleState != BattleState.None)
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
                        trueEvent.turner.tempStats.facing = trueEvent.direction;
                        participantModels[trueEvent.turner].transform.rotation = Quaternion.Euler(0, 90 * (int)trueEvent.direction, 0);
                        Debug.Log("Turned to face: " + 90 * (int)trueEvent.direction);
                    }
                }
                if (currentEvent is FunctionEvent)
                {
                    Debug.Log("EXECUTING A FUNCTION EVENT " + ((FunctionEvent)currentEvent).function.GetInvocationList()[0].Method.Name);
                    ((FunctionEvent)currentEvent).function();
                }
                if (currentEvent is FunctionEvent<BattlePawnBase, BattlePawnBase> && battleState != BattleState.None)
                {
                    Debug.Log("EXECUTING A FUNCTION EVENT<T1, T2>");
                    FunctionEvent<BattlePawnBase, BattlePawnBase> trueEvent = (FunctionEvent<BattlePawnBase, BattlePawnBase>)currentEvent;
                    trueEvent.function(trueEvent.first, trueEvent.second);
                }
            }
            else
            {
                switch (battleState)
                {
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
                    case BattleState.Player:
                    case BattleState.Attack:
                        //Shows whether the selected spell can be cast where the cursor is and what its range is
                        if (selectedSpell != -1)
                        {
                            updateTilesThisFrame = true;
                        }
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
                        if (n != -1)
                        {
                            players[n].tempStats.position = players[selectedPlayer].tempStats.position;
                            players[selectedPlayer].tempStats.position = pos;
                            participantModels[players[n]].transform.position = new Vector3(players[n].tempStats.position.x + bottomLeft.x, 1, players[n].tempStats.position.y + bottomLeft.y);
                            participantModels[players[selectedPlayer]].transform.position = new Vector3(
                                players[selectedPlayer].tempStats.position.x + bottomLeft.x,
                                1 + graphicalBattleMap.GetHeightAtGlobalPos(
                                    new Vector3(
                                        players[selectedPlayer].tempStats.position.x + bottomLeft.x,
                                        0,
                                        players[selectedPlayer].tempStats.position.y + bottomLeft.y)
                                    ),
                                players[selectedPlayer].tempStats.position.y + bottomLeft.y
                            );
                            actionTaken = true;
                        }
                        selectedPlayer = -1;
                        updateTilesThisFrame = true;
                    }
                    break;
                case BattleState.Player:
                    //If player is trying to move a pawn
                    if (battleMap[pos.x, pos.y].playerMoveRange && !players[selectedPlayer].tempStats.moved)
                    {
                        selectedMoveSpot.Set(pos.x, pos.y);
                        Vector2Int moveDifference = pos - players[selectedPlayer].tempStats.position;
                        graphicalBattleMap.ShowMoveMarker(moveDifference, pos, CanMoveYFirst(players[selectedPlayer], moveDifference));
                        actionTaken = true;
                    }
                    break;
            }
            //If player tries to cast a spell
            if (selectedSpell != -1)
            {
                if (skillLegitTarget && !skillCastConfirmMenu.activeSelf)
                {
                    selectedEnemy = EnemyAtPos(pos.x, pos.y);
                    spellCastPosition = new Vector2Int(pos.x, pos.y);
                    //Generates the choice menu
                    if (selectedMoveSpot.x != -1)
                        skillCastConfirmMenu.GetComponentInChildren<Text>().text = "You have a move selected. Move and cast?";
                    else
                        skillCastConfirmMenu.GetComponentInChildren<Text>().text = "Are you sure you want to cast there?";
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
                    graphicalBattleMap.HideMoveMarker();
                    selectedSpell = -1;
                }

                if (battleState != BattleState.Attack || battleMap[pos.x, pos.y].playerAttackRange)
                {
                    //Selecting an enemy
                    selectedEnemy = EnemyAtPos(pos.x, pos.y);
                    //If actually targetting another player for a healing attack
                    if (selectedEnemy == -1 && battleMap[pos.x, pos.y].playerAttackRange && battleState == BattleState.Attack && selectedSpell == -1)
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
        foreach (GameObject obj in participantModels.Values)
        {
            Destroy(obj);
        }
        participantModels = new Dictionary<BattlePawnBase, GameObject>();
        Destroy(battleCamera);
        battleCamera = null;
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
            graphicalBattleMap.EndOfBattle();
            eventQueue.Insert(new MovementEvent(mapPlayer.GetComponentInChildren<Camera>().gameObject, 4f, mapPlayer.GetComponentInChildren<Camera>().transform.position, true));
            eventQueue.Insert(new MovementEvent(mapPlayer.GetComponentInChildren<Camera>().gameObject, 4f, mapPlayer.GetComponentInChildren<Camera>().transform.rotation, true));
            mapPlayer.GetComponentInChildren<Camera>().transform.SetPositionAndRotation(battleCamera.transform.position, battleCamera.transform.rotation);
        }
        else
        {
        }
        ui.EndOfBattle();
        ExpungeAll();
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
            graphicalBattleMap.HideMoveMarker();
            updateTilesThisFrame = true;
            ui.UpdateSelectedUnit();
        }
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
            Vector2Int diff = selectedMoveSpot - players[selectedPlayer].tempStats.position;
            Debug.Log(diff);

            List<List<Vector3>> path = graphicalBattleMap.GetPath(
                diff,
                players[selectedPlayer].tempStats.position,
                1 + graphicalBattleMap.GetHeightAtGlobalPos(
                    new Vector3(
                        players[selectedPlayer].tempStats.position.x + bottomLeft.x,
                        0,
                        players[selectedPlayer].tempStats.position.y + bottomLeft.y)
                    ),
                CanMoveYFirst(players[selectedPlayer], diff)
            );

            foreach (List<Vector3> pointList in path)
            {
                if (pointList[1].x - pointList[0].x > 0)
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], FacingDirection.East));
                else if (pointList[1].x - pointList[0].x < 0)
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], FacingDirection.West));
                else if (pointList[1].z - pointList[0].z > 0)
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], FacingDirection.North));
                else
                    eventQueue.Insert(new TurnEvent(players[selectedPlayer], FacingDirection.South));

                for (int i = 0; i < pointList.Count; i++)
                {
                    pointList[i] += new Vector3(bottomLeft.x, 0, bottomLeft.y);
                }

                eventQueue.Insert(new MovementEvent(participantModels[players[selectedPlayer]], pawnMoveSpeed, pointList));
            }

            selectedMoveSpot = new Vector2Int(-1, -1);
            graphicalBattleMap.HideMoveMarker();
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
            players[selectedPlayer].tempStats.moved = true;
            selectedMoveSpot = new Vector2Int(-1, -1);
        }
        selectedPlayer = -1;
        selectedEnemy = -1;
        selectedSpell = -1;
        if (battleState != BattleState.None)
        {
            updateTilesThisFrame = true;
            battleState = BattleState.Player;

            //Checks if all players are done moving
            bool playersDone = true;
            foreach (Player p in players)
            {
                if (!p.tempStats.moved)
                    playersDone = false;
            }
            if (playersDone)
            {
                //Resets to start enemy moves
                for (int j = 0; j < enemies.Count; j++)
                {
                    enemies[j].tempStats.moved = !enemies[j].CanMove();
                }
                battleState = BattleState.Enemy;
            }
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
            graphicalBattleMap.HideMoveMarker();
            canSwap = false;
            foreach (Player p in players)
            {
                p.tempStats.moved = true;
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
         * Aggression determines their likelihood to take fights and whether they are scared of being outnumbered
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
                    possibleMoves.Add(new EnemyMove(pos.x, pos.y, aPos.x, aPos.y, 15 - (Mathf.Abs(pos.x - enemies[ID].tempStats.position.x) + Mathf.Abs(pos.y - enemies[ID].tempStats.position.y)), 1));
                }
            }
            foreach (Player p in players)
            {
                //If this move is the closest the enemy can get to a player, make it the move that happens if no attacks are possible
                if (Vector2Int.Distance(p.tempStats.position, pos) < fallbackMove.priority)
                {
                    fallbackMove = new EnemyMove(pos.x, pos.y, Vector2Int.Distance(p.tempStats.position, pos), 0);
                }
                //If this move ties with the current fallback move for distance from a player, pick a random one
                else if (Vector2Int.Distance(p.tempStats.position, pos) == fallbackMove.priority)
                {
                    if (Random.Range(0, 2) == 1)
                        fallbackMove = new EnemyMove(pos.x, pos.y, Vector2Int.Distance(p.tempStats.position, pos), 0);
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
        Vector2Int diff = possibleMoves[0].movePosition - enemies[ID].tempStats.position;

        List<List<Vector3>> path = graphicalBattleMap.GetPath(
                diff,
                enemies[ID].tempStats.position,
                1 + graphicalBattleMap.GetHeightAtGlobalPos(
                    new Vector3(
                        enemies[ID].tempStats.position.x + bottomLeft.x,
                        0,
                        enemies[ID].tempStats.position.y + bottomLeft.y)
                    ),
                CanMoveYFirst(enemies[ID], diff)
            );

        foreach (List<Vector3> pointList in path)
        {
            if (pointList[1].x - pointList[0].x > 0)
                eventQueue.Insert(new TurnEvent(enemies[ID], FacingDirection.East));
            else if (pointList[1].x - pointList[0].x < 0)
                eventQueue.Insert(new TurnEvent(enemies[ID], FacingDirection.West));
            else if (pointList[1].z - pointList[0].z > 0)
                eventQueue.Insert(new TurnEvent(enemies[ID], FacingDirection.North));
            else
                eventQueue.Insert(new TurnEvent(enemies[ID], FacingDirection.South));

            for (int i = 0; i < pointList.Count; i++)
            {
                pointList[i] += new Vector3(bottomLeft.x, 0, bottomLeft.y);
            }

            eventQueue.Insert(new MovementEvent(participantModels[enemies[ID]], pawnMoveSpeed, pointList));
        }
        enemies[ID].tempStats.moved = true;
        if (possibleMoves[0].attackPosition.x != -1)
            eventQueue.Insert(new FunctionEvent<BattlePawnBase, BattlePawnBase>(PerformAttack, enemies[movingEnemy], players[PlayerAtPos(possibleMoves[0].attackPosition.x, possibleMoves[0].attackPosition.y)]));
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
        temporaryTileType.EndOfTurn();
        foreach (Player p in players)
        {
            if (p.cHealth > 0)
            {
                p.tempStats.StartOfTurn();

                //Checks if any tile effects for ending a turn on a tile need to be done
                TriggerTileEffects(p, MoveTriggers.StartOfTurn);

                CheckEventTriggers(p, EffectTriggers.StartOfTurn);
                p.tempStats.moved = !p.CanMove();
            }
        }
        foreach (Enemy e in enemies)
        {
            if (e.cHealth > 0)
            {
                e.tempStats.StartOfTurn();

                //Checks if any tile effects for ending a turn on a tile need to be done
                TriggerTileEffects(e, MoveTriggers.StartOfTurn);

                CheckEventTriggers(e, EffectTriggers.StartOfTurn);
            }
        }
        temporaryTileType.StartOfTurn();
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
                p.tempStats.position.Set(-200, -200);
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
                e.tempStats.position.Set(-200, -200);
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
    /// <returns>The first value is the amount of damage, the second is the damage type</returns>
    public Pair<int, DamageType> GetDamageValues(BattlePawnBase attacker, BattlePawnBase target)
    {
        //Gets the distance between the player and enemy
        Vector2Int dist = target.tempStats.position - attacker.tempStats.position;

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
    public void PerformAttack(BattlePawnBase attacker, BattlePawnBase defender)
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

        eventQueue.Insert(new FunctionEvent<BattlePawnBase, BattlePawnBase>(PerformCounterattack, defender, attacker));
    }

    /// <summary>
    /// Performs a counterattack if one can be performed
    /// </summary>
    /// <param name="attacker">The counterattacker, aka the one who was hit by the initial attack</param>
    /// <param name="defender">The former attacker</param>
    public void PerformCounterattack(BattlePawnBase attacker, BattlePawnBase defender)
    {
        //If the defender lives and can attack back at that range
        if (attacker.cHealth > 0 && defender.cHealth > 0 && attacker.GetWeaponStatsAtDistance(attacker.tempStats.position - defender.tempStats.position) != null)
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

            eventQueue.Insert(new FunctionEvent<BattlePawnBase, BattlePawnBase>(CheckForKill, attacker, defender));
        }
    }

    /// <summary>
    /// Checks to see if a pawn dies from a source of damage and checks for relevant triggers
    /// </summary>
    /// <param name="attacker">The pawn that may have killed another</param>
    /// <param name="defender">The target that may have died</param>
    private void CheckForKill(BattlePawnBase attacker, BattlePawnBase defender)
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
            if (players[id].tempStats.position.x == x && players[id].tempStats.position.y == y)
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
            if (enemies[e].tempStats.position.x == x && enemies[e].tempStats.position.y == y)
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
        for (int x = 0; x < BattleMap.mapSizeX; x++)
        {
            for (int y = 0; y < BattleMap.mapSizeY; y++)
            {
                battleMap[x, y].Reset();
            }
        }

        if (battleState == BattleState.Swap)
        {
            foreach (Player p in players)
            {
                battleMap[p.tempStats.position.x, p.tempStats.position.y].playerMoveRange = true;
                battleMap[p.tempStats.position.x, p.tempStats.position.y].enemyDanger = true;
            }
            if (selectedPlayer != -1)
                battleMap[players[selectedPlayer].tempStats.position.x, players[selectedPlayer].tempStats.position.y].enemyDanger = false;
        }

        //Shows skill range and what is targettable within that range if a spell is selected or hovered
        if (selectedSpell != -1 || hoveredSpell != -1)
        {
            int skillToShow = selectedSpell;
            if (hoveredSpell != -1)
                skillToShow = hoveredSpell;
            Vector2Int skillPos = players[selectedPlayer].tempStats.position;
            if (selectedMoveSpot.x != -1)
                skillPos = selectedMoveSpot;
            Skill displaySkill = Registry.SpellTreeRegistry[players[selectedPlayer].skillQuickList[skillToShow].x][players[selectedPlayer].skillQuickList[skillToShow].y];

            if (displaySkill.targetType == TargettingType.Self)
            {
                battleMap[skillPos.x, skillPos.y].skillTargettable = true;
            }
            else
            {
                for (int x = -displaySkill.targettingRange; x <= displaySkill.targettingRange; x++)
                {
                    for (int y = -displaySkill.targettingRange; y <= displaySkill.targettingRange; y++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= displaySkill.targettingRange && x + skillPos.x >= 0 && x + skillPos.x < BattleMap.mapSizeX && y + skillPos.y >= 0 && y + skillPos.y < BattleMap.mapSizeY)
                        {
                            if (displaySkill.targetType == TargettingType.AllInRange || displaySkill.targetType == TargettingType.Ally && PlayerAtPos(x + skillPos.x, y + skillPos.y) != -1 || displaySkill.targetType == TargettingType.Enemy && EnemyAtPos(x + skillPos.x, y + skillPos.y) != -1)
                                battleMap[x + skillPos.x, y + skillPos.y].skillTargettable = true;
                            else
                                battleMap[x + skillPos.x, y + skillPos.y].skillRange = true;
                        }
                    }
                }
            }
            //Shows whether the selected spell can be cast where the cursor is and what its range is
            if (selectedSpell != -1)
            {
                int layerMask = 1 << 8;
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    Vector2Int pos = graphicalBattleMap.GetInteractionPos(hit.point);
                    skillLegitTarget = battleMap[pos.x, pos.y].skillTargettable;

                    Debug.Log(pos);
                    for (int x = -Mathf.FloorToInt((displaySkill.xRange - 1) / 2.0f); x <= Mathf.CeilToInt((displaySkill.xRange - 1) / 2.0f); x++)
                    {
                        for (int y = -Mathf.FloorToInt((displaySkill.yRange - 1) / 2.0f); y <= Mathf.CeilToInt((displaySkill.yRange - 1) / 2.0f); y++)
                        {
                            if (x + pos.x >= 0 && x + pos.x < BattleMap.mapSizeX && y + pos.y >= 0 && y + pos.y < BattleMap.mapSizeY)
                                battleMap[x + pos.x, y + pos.y].skillTargetting = true;
                        }
                    }
                    updateTilesThisFrame = true;
                }
            }
        }

        //Renders where a valid attack spot would be for a player after they move
        else if (battleState == BattleState.Attack)
        {
            WeaponType weapon = Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[players[selectedPlayer].equippedWeapon.Name]).subType];

            List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, players[selectedPlayer].tempStats.position);
            foreach (Vector2Int attackPos in attackSpots)
            {
                //If the player can attack an enemy or heal an ally at that position
                if (EnemyAtPos(attackPos.x, attackPos.y) != -1 || (PlayerAtPos(attackPos.x, attackPos.y) != -1 && weapon.GetStatsAtRange(attackPos - players[selectedPlayer].tempStats.position).heals))
                    battleMap[attackPos.x, attackPos.y].playerAttackRange = true;
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
                    battleMap[pos.x, pos.y].playerMoveRange = true;
                    List<Vector2Int> attackSpots = GetViableAttackSpaces(weapon, pos);
                    foreach (Vector2Int attackPos in attackSpots)
                    {
                        battleMap[attackPos.x, attackPos.y].playerAttackRange = true;
                    }
                }
            }
            //If there is a selected enemy but no selected player, render the enemy's ranges
            else if (selectedEnemy != -1)
            {
                List<Vector2Int> moveSpots = GetViableMovements(enemies[selectedEnemy]);
                foreach (Vector2Int pos in moveSpots)
                {
                    battleMap[pos.x, pos.y].enemyDanger = true;
                    List<Vector2Int> attackSpots = GetViableAttackSpaces(Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[enemies[selectedEnemy].equippedWeapon.Name]).subType], pos);
                    foreach (Vector2Int attackPos in attackSpots)
                    {
                        battleMap[attackPos.x, attackPos.y].enemyDanger = true;
                    }
                }
            }
        }

        //Calculates the movement and attack ranges of enemies
        foreach (Enemy e in enemies)
        {
            List<Vector2Int> moveSpots = GetViableMovements(e);
            foreach (Vector2Int pos in moveSpots)
            {
                battleMap[pos.x, pos.y].enemyDanger = true;
                List<Vector2Int> attackSpots = GetViableAttackSpaces(Registry.WeaponTypeRegistry[((EquippableBase)Registry.ItemRegistry[e.equippedWeapon.Name]).subType], pos);
                foreach (Vector2Int attackPos in attackSpots)
                {
                    battleMap[attackPos.x, attackPos.y].enemyDanger = true;
                }
            }
        }
        graphicalBattleMap.UpdateVisuals();
        updateTilesThisFrame = false;
    }

    /// <summary>
    /// Returns whether or not the path a pawn can take to a given position can be vertical first
    /// </summary>
    /// <param name="mover">The pawn that is moving</param>
    /// <param name="relativeMove">The position they want to move to relative to their current position</param>
    private bool CanMoveYFirst(BattlePawnBase mover, Vector2Int relativeMove)
    {
        for (int y = 0; y <= Mathf.Abs(relativeMove.y); y++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.tempStats.position.x, mover.tempStats.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))].tileType))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.tempStats.position.x, mover.tempStats.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))) : PlayerAtPos(mover.tempStats.position.x, mover.tempStats.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y)))) != -1)
                return false;
        }

        for (int x = 0; x <= Mathf.Abs(relativeMove.x); x++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.tempStats.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.tempStats.position.y + relativeMove.y].tileType))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.tempStats.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.tempStats.position.y + relativeMove.y) : PlayerAtPos(mover.tempStats.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.tempStats.position.y + relativeMove.y)) != -1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns whether or not the path a pawn can take to a given position can be horizontal first
    /// </summary>
    /// <param name="mover">The pawn that is moving</param>
    /// <param name="relativeMove">The position they want to move to relative to their current position</param>
    private bool CanMoveXFirst(BattlePawnBase mover, Vector2Int relativeMove)
    {
        for (int x = 0; x <= Mathf.Abs(relativeMove.x); x++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.tempStats.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.tempStats.position.y].tileType))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.tempStats.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.tempStats.position.y) : PlayerAtPos(mover.tempStats.position.x + x * Mathf.RoundToInt(Mathf.Sign(relativeMove.x)), mover.tempStats.position.y)) != -1)
                return false;
        }

        for (int y = 0; y <= Mathf.Abs(relativeMove.y); y++)
        {
            if (!mover.ValidMoveTile(battleMap[mover.tempStats.position.x + relativeMove.x, mover.tempStats.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))].tileType))
                return false;
            if ((mover is Player ? EnemyAtPos(mover.tempStats.position.x + relativeMove.x, mover.tempStats.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y))) : PlayerAtPos(mover.tempStats.position.x + relativeMove.x, mover.tempStats.position.y + y * Mathf.RoundToInt(Mathf.Sign(relativeMove.y)))) != -1)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Gets all of the places a given pawn can move to
    /// </summary>
    /// <param name="entity">The entity moving</param>
    /// <returns>First = a valid position, Second = whether or not moving vertically first is valid</returns>
    private List<Vector2Int> GetViableMovements(BattlePawnBase entity)
    {
        List<Vector2Int> moveSpots = new List<Vector2Int>();
        bool isPlayer = entity is Player;
        int maxMove = entity.GetEffectiveStat(Stats.MaxMove);
        for (int x = -maxMove; x <= maxMove; x++)
        {
            for (int y = -maxMove; y <= maxMove; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= maxMove && x + entity.tempStats.position.x >= 0 && x + entity.tempStats.position.x < BattleMap.mapSizeX && y + entity.tempStats.position.y >= 0 && y + entity.tempStats.position.y < BattleMap.mapSizeY)
                {
                    //It is automatically valid if the entity is moving to itself
                    if (x == 0 && y == 0)
                    {
                        moveSpots.Add(new Vector2Int(entity.tempStats.position.x, entity.tempStats.position.y));
                        continue;
                    }
                    //It is an invalid move position if it would overlap with an existing entity
                    if (PlayerAtPos(x + entity.tempStats.position.x, y + entity.tempStats.position.y) != -1)
                        continue;
                    if (EnemyAtPos(x + entity.tempStats.position.x, y + entity.tempStats.position.y) != -1)
                        continue;

                    if (CanMoveYFirst(entity, new Vector2Int(x, y)) || CanMoveXFirst(entity, new Vector2Int(x, y)))
                        moveSpots.Add(new Vector2Int(x + entity.tempStats.position.x, y + entity.tempStats.position.y));
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
            if (statsAtRange.atDistance >= 1)
            {
                bool validLeftTile = true;
                bool validRightTile = true;
                bool validUpTile = true;
                bool validDownTile = true;
                //Checks to see if the attack's paths is unobstructed
                for (int dist = 1; dist <= statsAtRange.atDistance && (validLeftTile || validRightTile || validUpTile || validDownTile); dist++)
                {
                    if (validLeftTile && (center.x - statsAtRange.atDistance < 0 ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x - dist, center.y].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x - dist, center.y].tileType].blocksRangedAttacks)))
                    {
                        validLeftTile = false;
                    }

                    if (validRightTile && (center.x + statsAtRange.atDistance >= BattleMap.mapSizeX ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x + dist, center.y].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x + dist, center.y].tileType].blocksRangedAttacks)))
                    {
                        validRightTile = false;
                    }

                    if (validUpTile && (center.y - statsAtRange.atDistance < 0 ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x, (center.y - dist)].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x, (center.y - dist)].tileType].blocksRangedAttacks)))
                    {
                        validUpTile = false;
                    }

                    if (validDownTile && (center.y + statsAtRange.atDistance >= BattleMap.mapSizeY ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x, (center.y + dist)].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x, (center.y + dist)].tileType].blocksRangedAttacks)))
                    {
                        validDownTile = false;
                    }
                }
                if (validLeftTile)
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, center.y));
                if (validRightTile)
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, center.y));
                if (validUpTile)
                    attackSpots.Add(new Vector2Int(center.x, (center.y - statsAtRange.atDistance)));
                if (validDownTile)
                    attackSpots.Add(new Vector2Int(center.x, (center.y + statsAtRange.atDistance)));
            }
        }
        //Gets all of the diagonal viable attack position
        foreach (WeaponStatsAtRange statsAtRange in weapon.diagonalRanges)
        {
            if (statsAtRange.atDistance >= 1)
            {
                bool validUpLeftTile = true;
                bool validUpRightTile = true;
                bool validDownLeftTile = true;
                bool validDownRightTile = true;
                //Checks to see if the attack's paths is unobstructed
                for (int dist = 1; dist <= statsAtRange.atDistance && (validUpLeftTile || validUpRightTile || validDownLeftTile || validDownRightTile); dist++)
                {
                    if (validUpLeftTile && (center.x - statsAtRange.atDistance < 0 || center.y + statsAtRange.atDistance >= BattleMap.mapSizeY ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x - dist, (center.y - statsAtRange.atDistance)].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x - dist, (center.y - statsAtRange.atDistance)].tileType].blocksRangedAttacks)))
                    {
                        validUpLeftTile = false;
                    }

                    if (validUpRightTile && (center.x + statsAtRange.atDistance >= BattleMap.mapSizeX || center.y + statsAtRange.atDistance >= BattleMap.mapSizeY ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x + dist, (center.y - statsAtRange.atDistance)].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x + dist, (center.y - statsAtRange.atDistance)].tileType].blocksRangedAttacks)))
                    {
                        validUpRightTile = false;
                    }

                    if (validDownLeftTile && (center.x - statsAtRange.atDistance < 0 || center.y + statsAtRange.atDistance < 0 ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x - dist, (center.y + dist)].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x - dist, (center.y + dist)].tileType].blocksRangedAttacks)))
                    {
                        validDownLeftTile = false;
                    }

                    if (validDownRightTile && (center.x + statsAtRange.atDistance >= BattleMap.mapSizeX || center.y + statsAtRange.atDistance < 0 ||
                        (!statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x + dist, (center.y + dist)].tileType].blocksMeleeAttacks ||
                        statsAtRange.ranged && Registry.TileTypeRegistry[battleMap[center.x + dist, (center.y + dist)].tileType].blocksRangedAttacks)))
                    {
                        validDownRightTile = false;
                    }
                }
                if (validUpLeftTile)
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, (center.y - statsAtRange.atDistance)));
                if (validUpRightTile)
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, (center.y - statsAtRange.atDistance)));
                if (validDownLeftTile)
                    attackSpots.Add(new Vector2Int(center.x - statsAtRange.atDistance, (center.y + statsAtRange.atDistance)));
                if (validDownRightTile)
                    attackSpots.Add(new Vector2Int(center.x + statsAtRange.atDistance, (center.y + statsAtRange.atDistance)));
            }
        }
        return attackSpots;
    }

    /// <summary>
    /// Activates all the triggers on a pawn from the tile they're on
    /// </summary>
    /// <param name="pawn">What pawn is on this tile</param>
    /// <param name="trigger">The type of effects to trigger</param>
    private void TriggerTileEffects(BattlePawnBase pawn, MoveTriggers trigger)
    {
        List<TileType> effects = new List<TileType>();
        if (Registry.TileTypeRegistry[battleMap[pawn.tempStats.position.x, pawn.tempStats.position.y].tileType].Contains(trigger))
            effects.Add(Registry.TileTypeRegistry[battleMap[pawn.tempStats.position.x, pawn.tempStats.position.y].tileType]);
        effects.AddRange(temporaryTileType.GetTileType(pawn.tempStats.position, trigger));
        foreach (TileType effect in effects)
        {
            ExecuteEffectEvent copiedEffect = effect[trigger];
            copiedEffect.target = pawn;
            eventQueue.Insert(copiedEffect);
        }
    }

    /// <summary>
    /// Checks for and executes all events triggered by the given trigger
    /// </summary>
    /// <param name="triggered">The pawn we are checking</param>
    /// <param name="trigger">The type of trigger that was tripped</param>
    /// <param name="other">If the event involved another pawn, such as taking damage or giving healing</param>
    /// <param name="data">If the event has other important data, such as the amount of damage taken</param>
    private void CheckEventTriggers(BattlePawnBase triggered, EffectTriggers trigger, BattlePawnBase other = null, int data = -1)
    {
        List<SkillPartBase> list = triggered.tempStats.GetTriggeredEffects(trigger);
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
    public void ExecuteEffect(SkillPartBase effect, BattlePawnBase caster, BattlePawnBase target, bool fromSpell = false, int valueFromPrevious = -1)
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
                damage += target.Damage(Mathf.RoundToInt(trueEffect.damage * mod *
                    (trueEffect.damageType == DamageType.Physical ? caster.GetEffectiveStat(Stats.Attack) * 3.0f / (target.GetEffectiveStat(Stats.Defense) * ((100 - caster.GetEffectiveStat(Stats.PercentArmorPierce)) / 100.0f) - caster.GetEffectiveStat(Stats.FlatArmorPierce)) :
                    caster.GetEffectiveStat(Stats.MagicAttack) * 3.0f / (target.GetEffectiveStat(Stats.MagicDefense) * ((100 - caster.GetEffectiveStat(Stats.PercentMArmorPierce)) / 100.0f) - caster.GetEffectiveStat(Stats.FlatMArmorPierce)))));
                damage += target.Damage((int)((target.GetEffectiveStat(Stats.MaxHealth) - target.cHealth) * 1.0f / target.GetEffectiveStat(Stats.MaxHealth) * trueEffect.missingHpPercent * mod / 100.0f));
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
                eventQueue.Insert(new FunctionEvent<BattlePawnBase, BattlePawnBase>(CheckForKill, caster, target));

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
                if (trueEffect.statMod.flatChange < 0)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.statMod.affectedStat) + " was decreased by " + trueEffect.statMod.flatChange + "!"));
                else if (trueEffect.statMod.flatChange > 0)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.statMod.affectedStat) + " was increased by " + trueEffect.statMod.flatChange + "!"));
                if (trueEffect.statMod.multiplier < 1)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.statMod.affectedStat) + " was decreased by " + ((1 - trueEffect.statMod.flatChange) * 100) + "%!"));
                else if (trueEffect.statMod.multiplier > 1)
                    eventQueue.Insert(new TextEvent(target.name + "'s " + GameStorage.StatToString(trueEffect.statMod.affectedStat) + " was increased by " + ((trueEffect.statMod.flatChange - 1) * 100) + "%!"));
                target.tempStats.AddMod(trueEffect.statMod);
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
                target.tempStats.AddTemporaryTrigger(trueEffect.effect, trueEffect.maxTimesThisBattle, trueEffect.turnCooldown, trueEffect.maxActiveTurns);
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
                    Vector2Int diff = target.tempStats.position - trueEffect.center;
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
                        if (target.ValidMoveTile(battleMap[target.tempStats.position.x, target.tempStats.position.y + i * dir].tileType))
                            eventQueue.Insert(new MovementEvent(participantModels[target], pawnMoveSpeed,
                                new List<Vector3>{ participantModels[target].transform.position + new Vector3Int(0, 0, i - 1) * dir,
                                participantModels[target].transform.position + new Vector3Int(0, 0, i) * dir }));
                        else
                        {
                            //If the pawn hits a spot where they can't move any further, stun them for a turn
                            eventQueue.Insert(new ExecuteEffectEvent(new StatusEffectPart(TargettingType.AllInRange, "Stun", false), caster, target));
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
                        if (target.ValidMoveTile(battleMap[target.tempStats.position.x + i * dir, target.tempStats.position.y].tileType))
                            eventQueue.Insert(new MovementEvent(participantModels[target], pawnMoveSpeed,
                                new List<Vector3>{ participantModels[target].transform.position + new Vector3Int(i - 1, 0, 0) * dir,
                                participantModels[target].transform.position + new Vector3Int(i, 0, 0) * dir }));
                        else
                        {
                            //If the pawn hits a spot where they can't move any further, stun them for a turn
                            eventQueue.Insert(new ExecuteEffectEvent(new StatusEffectPart(TargettingType.AllInRange, "Stun", false), caster, target));
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
                        if (target.tempStats.moved)
                        {
                            //Check if the target is able to move again
                            bool canMove = target.CanMove();
                            target.tempStats.moved = !canMove;
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
    public void CastSkill(Skill castedSpell, int castedX, int castedY, BattlePawnBase caster)
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
        Skill displaySkill = Registry.SpellTreeRegistry[players[selectedPlayer].skillQuickList[selectedSpell].x][players[selectedPlayer].skillQuickList[selectedSpell].y];
        CastSkill(displaySkill, spellCastPosition.x, spellCastPosition.y, players[selectedPlayer]);
        eventQueue.Insert(new FunctionEvent(FinishedMovingPawn));
        selectedSpell = -1;
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
