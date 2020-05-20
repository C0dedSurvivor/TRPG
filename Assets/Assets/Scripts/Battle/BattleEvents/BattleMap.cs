using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The terrain layer for each color
/// </summary>
enum TileColors
{
    None,
    PlayerTraversable,
    PlayerAttackable,
    PlayerMoveIntoDanger,
    Danger,
    SpellRange,
    ValidSpellTarget,
    TargettingValidSpellTarget,
    TargettingInvalidSpellTarget
}

public class BattleMap : MonoBehaviour
{
    //Prefabs
    public GameObject MoveMarkerPrefab;
    public GameObject aEtherMarkerPrefab;

    //Declares the map size, unchanged post initialization. 
    //Default is 20x20, camera will not change view to accomodate larger currently
    public const int mapSizeX = 20;
    public const int mapSizeY = 20;
    public float mapVertOffset => transform.position.y;

    //The length of one tile on the map
    const int tileSize = 51;

    public bool showDanger = false;
    public bool showaEther = false;

    public Terrain map;
    public Battle battle;
    public GameObject[,] aEtherMap;

    TileColors[,] currentVisuals;

    //This displays how the pawn would move when a move is selected
    public GameObject moveMarker;

    //How many positions should be generated along a unit length of path
    public float subdivisionsPerUnit = 10.0f;
    public float subdivisionIncrement => 1.0f / subdivisionsPerUnit;

    /// <summary>
    /// Instantiates the movement marker and hides all battle-related objects
    /// </summary>
    public void Start()
    {
        map = GetComponent<Terrain>();
        //Creates the move marker for the player
        moveMarker = Instantiate(MoveMarkerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
        moveMarker.SetActive(false);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Checks for the player clicking on the battle map
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int layerMask = 1 << 8;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Vector2Int interactionPos = GetInteractionPos(hit.point);
                Debug.Log("X: " + interactionPos.x + "  Y: " + interactionPos.y);
                battle.SpaceInteraction(interactionPos);
            }
        }
    }

    /// <summary>
    /// Get where the player clicked in battle coordinates
    /// </summary>
    /// <param name="hitPoint">Hit position in world coordinates</param>
    /// <returns>Coordinates that were clicked on</returns>
    public Vector2Int GetInteractionPos(Vector3 hitPoint)
    {
        Vector3 localHitPos = hitPoint - transform.position;
        return new Vector2Int((int)localHitPos.x, (int)localHitPos.z);
    }

    /// <summary>
    /// Grabs the appropriate heightmap and clears the texture of the battle map for the start of the battle
    /// </summary>
    /// <param name="xPos">X position of the map</param>
    /// <param name="yPos">Y position of the map</param>
    public void StartOfBattle(int xPos, int yPos)
    {
        transform.position = new Vector3(xPos - 0.5f, 0.2f, yPos - 0.5f);
        gameObject.SetActive(true);
        TerrainData terrainData = map.terrainData;

        float[,] sourceHeights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        for (int x = 0; x < mapSizeX * tileSize; x++)
        {
            for (int y = 0; y < mapSizeY * tileSize; y++)
            {
                sourceHeights[y, x] = (GameStorage.mapTerrain.SampleHeight(transform.position + new Vector3(x / 51.0f, 0, y / 51.0f)) - GameStorage.mapTerrain.transform.position.y)
                    / GameStorage.mapTerrain.terrainData.heightmapScale.y;
            }
        }
        map.terrainData.SetHeights(0, 0, sourceHeights);

        currentVisuals = new TileColors[mapSizeX, mapSizeY];

        //Resets the initial battle visuals
        //get current paint mask
        float[,,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        aEtherMap = new GameObject[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                for (int xPixel = x * tileSize + 2; xPixel < (x + 1) * tileSize + 2; xPixel++)
                {
                    for (int yPixel = y * tileSize + 2; yPixel < (y + 1) * tileSize + 2; yPixel++)
                    {
                        alphas[yPixel, xPixel, (int)TileColors.None] = 0.8f;
                        alphas[yPixel, xPixel, (int)TileColors.PlayerTraversable] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.PlayerAttackable] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.PlayerMoveIntoDanger] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.Danger] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.SpellRange] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.ValidSpellTarget] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.TargettingValidSpellTarget] = 0f;
                        alphas[yPixel, xPixel, (int)TileColors.TargettingInvalidSpellTarget] = 0f;
                    }
                }

                //Creates the aEther map visuals
                aEtherMap[x, y] = Instantiate(aEtherMarkerPrefab);
                aEtherMap[x, y].transform.position = new Vector3(xPos + x + 0.5f, GetHeightAtGlobalPos(new Vector3(x + transform.position.x, 0.2f, y + transform.position.z)) + 0.5f, yPos + y + 0.5f);
                aEtherMap[x, y].SetActive(false);
            }
        }
        // apply the new alpha
        terrainData.SetAlphamaps(0, 0, alphas);

        UpdateVisuals();
    }

    /// <summary>
    /// Destroys the aEther map and hides battle map and move marker at the end of a battle
    /// </summary>
    public void EndOfBattle()
    {
        gameObject.SetActive(false);
        moveMarker.SetActive(false);

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Destroy(aEtherMap[x, y]);
            }
        }
        aEtherMap = null;
    }

    /// <summary>
    /// Updates the textures of any tiles changed between last render and this one
    /// </summary>
    public void UpdateVisuals()
    {
        TerrainData terrainData = map.terrainData;
        //get current paint mask
        float[,,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        // make sure every grid on the terrain is modified
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                TileColors color = TileColors.None;
                if (battle.battleMap[x, y].skillTargetting)
                {
                    if (battle.skillLegitTarget)
                        color = TileColors.TargettingValidSpellTarget;
                    else
                        color = TileColors.TargettingInvalidSpellTarget;
                }
                else if (battle.battleMap[x, y].skillTargettable)
                {
                    color = TileColors.ValidSpellTarget;
                }
                else if (battle.battleMap[x, y].skillRange)
                {
                    color = TileColors.SpellRange;
                }
                else if (battle.battleMap[x, y].enemyDanger && showDanger && battle.battleMap[x, y].playerMoveRange)
                {
                    color = TileColors.PlayerMoveIntoDanger;
                }
                else if (battle.battleMap[x, y].playerMoveRange)
                {
                    color = TileColors.PlayerTraversable;

                }
                else if (battle.battleMap[x, y].playerAttackRange)
                {
                    color = TileColors.PlayerAttackable;
                }
                else if (battle.battleMap[x, y].enemyDanger && showDanger)
                {
                    color = TileColors.Danger;
                }

                if (color != currentVisuals[x, y])
                {
                    UpdateSingleTile(x, y, alphas, color, currentVisuals[x, y]);
                    currentVisuals[x, y] = color;
                }
            }
        }
        // apply the new alpha
        terrainData.SetAlphamaps(0, 0, alphas);
    }

    /// <summary>
    /// Updates the image for a single tile's worth of area on the battle map
    /// </summary>
    /// <param name="xPos">X position in battle coordinates</param>
    /// <param name="yPos">Y position in battle coordinates</param>
    /// <param name="alphas">Alpha map to update each point with</param>
    /// <param name="color">What new tile type to show</param>
    /// <param name="previousColor">What old tile type to wipe it of</param>
    private void UpdateSingleTile(int xPos, int yPos, float[,,] alphas, TileColors color, TileColors previousColor)
    {
        for (int x = xPos * tileSize + 2; x < (xPos + 1) * tileSize + 2; x++)
        {
            for (int y = yPos * tileSize + 2; y < (yPos + 1) * tileSize + 2; y++)
            {
                alphas[y, x, (int)previousColor] = 0f;

                alphas[y, x, (int)color] = 0.8f;
            }
        }
    }

    /// <summary>
    /// Updates the aEther map with the current aEther levels to be rendered
    /// </summary>
    private void UpdateaEtherMap()
    {
        //Updates the aEther viewer
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                aEtherMap[x, y].SetActive(showaEther);
                if (showaEther)
                    aEtherMap[x, y].transform.localScale = new Vector3(0.1f * battle.aEtherMap[x, y, 0], 0.01f, 0.1f * battle.aEtherMap[x, y, 0]);
            }
        }
    }

    /// <summary>
    /// Shows the move marker with a line going from it to the BattlePawn along the path the BattlePawn would take
    /// </summary>
    /// <param name="moveDifference">The path lengths to follow</param>
    /// <param name="markerPos">The ending position in battle coordinates (Where the marker is)</param>
    /// <param name="verticalFirst">Whether the path should do vertical or horizontal first</param>
    public void ShowMoveMarker(Vector2Int moveDifference, Vector2Int markerPos, bool verticalFirst)
    {
        moveMarker.SetActive(true);

        //Update the line renderer
        LineRenderer path = moveMarker.GetComponent<LineRenderer>();

        moveMarker.transform.position = new Vector3(
            markerPos.x + battle.bottomLeft.x,
            1 + GetHeightAtGlobalPos(new Vector3(markerPos.x + battle.bottomLeft.x, 0, markerPos.y + battle.bottomLeft.y)),
            markerPos.y + battle.bottomLeft.y
        );

        List<List<Vector3>> linePositions = GetPath(-moveDifference, markerPos, 0, !verticalFirst);
        List<Vector3> linePath = new List<Vector3>();
        foreach (List<Vector3> pointList in linePositions)
        {
            foreach (Vector3 point in pointList)
            {
                Vector3 scaledPathPos = point - new Vector3(markerPos.x, moveMarker.transform.position.y - 1, markerPos.y);
                scaledPathPos.x *= 1.0f / moveMarker.transform.lossyScale.x;
                scaledPathPos.y *= 1.0f / moveMarker.transform.lossyScale.y;
                scaledPathPos.z *= 1.0f / moveMarker.transform.lossyScale.z;
                linePath.Add(scaledPathPos);
            }
        }
        path.positionCount = linePath.Count;
        path.SetPositions(linePath.ToArray());
    }

    /// <summary>
    /// Gets the global height of a position on the battle map's heightmap
    /// </summary>
    /// <param name="pos">Position in global coordinates</param>
    /// <returns>Global height of the point on the battle map heightmap</returns>
    public float GetHeightAtGlobalPos(Vector3 pos)
    {
        return map.SampleHeight(pos);
    }

    /// <summary>
    /// Generates a path from a position following an offset
    /// </summary>
    /// <param name="difference">The path lengths to follow</param>
    /// <param name="startingPos">The starting position in battle coordinates</param>
    /// <param name="verticalOffset">The initial vertical offset of the object following the path</param>
    /// <param name="verticalFirst">Whether the path should do vertical or horizontal first</param>
    /// <returns></returns>
    public List<List<Vector3>> GetPath(Vector2Int difference, Vector2Int startingPos, float verticalOffset, bool verticalFirst)
    {
        List<List<Vector3>> movementList = new List<List<Vector3>>();

        if (verticalFirst)
        {
            for (int y = 0; y < Mathf.Abs(difference.y); y++)
            {
                List<Vector3> positionList = new List<Vector3>();
                for (float ySlice = y; ySlice <= y + 1 + subdivisionIncrement / 2; ySlice += subdivisionIncrement)
                {
                    float zPos = ySlice * Mathf.Sign(difference.y) + startingPos.y;
                    positionList.Add(new Vector3(startingPos.x, verticalOffset + GetHeightAtGlobalPos(new Vector3(startingPos.x + transform.position.x + 0.5f, 0, transform.position.z + zPos + 0.5f)), zPos));
                }
                movementList.Add(positionList);
            }
            for (int x = 0; x < Mathf.Abs(difference.x); x++)
            {
                List<Vector3> positionList = new List<Vector3>();
                float zPos = difference.y + startingPos.y;
                for (float xSlice = x; xSlice <= x + 1 + subdivisionIncrement / 2; xSlice += subdivisionIncrement)
                {
                    float xPos = xSlice * Mathf.Sign(difference.x) + startingPos.x;
                    positionList.Add(new Vector3(xPos, verticalOffset + GetHeightAtGlobalPos(new Vector3(xPos + transform.position.x + 0.5f, 0, transform.position.z + zPos + 0.5f)), zPos));
                }
                movementList.Add(positionList);
            }
        }
        else
        {
            for (int x = 0; x < Mathf.Abs(difference.x); x++)
            {
                List<Vector3> positionList = new List<Vector3>();
                for (float xSlice = x; xSlice <= x + 1 + subdivisionIncrement / 2; xSlice += subdivisionIncrement)
                {
                    float xPos = xSlice * Mathf.Sign(difference.x) + startingPos.x;
                    positionList.Add(new Vector3(xPos, verticalOffset + GetHeightAtGlobalPos(new Vector3(xPos + transform.position.x + 0.5f, 0, transform.position.z + startingPos.y + 0.5f)), startingPos.y));
                }
                movementList.Add(positionList);
            }
            for (int y = 0; y < Mathf.Abs(difference.y); y++)
            {
                List<Vector3> positionList = new List<Vector3>();
                float xPos = difference.x + startingPos.x;
                for (float ySlice = y; ySlice <= y + 1 + subdivisionIncrement / 2; ySlice += subdivisionIncrement)
                {
                    float zPos = ySlice * Mathf.Sign(difference.y) + startingPos.y;
                    positionList.Add(new Vector3(xPos, verticalOffset + GetHeightAtGlobalPos(new Vector3(xPos + transform.position.x + 0.5f, 0, transform.position.z + zPos + 0.5f)), zPos));
                }
                movementList.Add(positionList);
            }
        }

        return movementList;
    }

    /// <summary>
    /// Hides the move marker once a move is deselected or completed
    /// </summary>
    public void HideMoveMarker()
    {
        moveMarker.SetActive(false);
    }

    /// <summary>
    /// Toggles whether enemy ranges are shown or not
    /// </summary>/
    public void ToggleDangerArea()
    {
        showDanger = !showDanger;
        battle.updateTilesThisFrame = true;
    }

    /// <summary>
    /// Toggles whether the aEther visual representation is shown or not
    /// </summary>
    public void ToggleaEtherView()
    {
        showaEther = !showaEther;
        UpdateaEtherMap();
    }
}