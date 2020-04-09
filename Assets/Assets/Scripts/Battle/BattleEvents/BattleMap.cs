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

    public void Start()
    {
        map = GetComponent<Terrain>();
        //Creates the move marker for the player
        moveMarker = Instantiate(MoveMarkerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
        moveMarker.SetActive(false);
        gameObject.SetActive(false);
    }

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

    public Vector2Int GetInteractionPos(Vector3 hitPoint)
    {
        Vector3 localHitPos = hitPoint - transform.position;
        return new Vector2Int((int)localHitPos.x, mapSizeY - (int)localHitPos.z - 1);
    }

    public void StartOfBattle(int xPos, int yPos)
    {
        transform.position = new Vector3(xPos - 0.5f, 0.01f, yPos - 0.5f);
        gameObject.SetActive(true);

        float mapSizeRatio = map.terrainData.size.x / GameStorage.mapTerrain.terrainData.size.x;
        float[,] sourceHeights = GameStorage.mapTerrain.terrainData.GetHeights(xPos, yPos, Mathf.RoundToInt(GameStorage.mapTerrain.terrainData.heightmapResolution * mapSizeRatio), Mathf.RoundToInt(GameStorage.mapTerrain.terrainData.heightmapResolution * mapSizeRatio));
        map.terrainData.SetHeights(0, 0, sourceHeights);

        currentVisuals = new TileColors[mapSizeX, mapSizeY];

        //Resets the initial battle visuals
        TerrainData terrainData = map.terrainData;
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
                aEtherMap[x, y].transform.position = new Vector3(xPos + x + 0.5f, GameStorage.mapTerrain.terrainData.GetHeight(x, y) + 0.5f, yPos + (mapSizeY - y - 0.5f));
                aEtherMap[x, y].SetActive(false);
            }
        }
        // apply the new alpha
        terrainData.SetAlphamaps(0, 0, alphas);

        UpdateVisuals();
    }

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

    public void ShowMoveMarker(Vector2Int moveDifference, Vector2Int markerPos, bool verticalFirst)
    {
        moveMarker.transform.position = new Vector3(markerPos.x, 1, markerPos.y);
        moveMarker.SetActive(true);

        //Update the line renderer
        
        LineRenderer path = moveMarker.GetComponent<LineRenderer>();

        Vector2Int flatOffset = markerPos - battle.topLeft;
        Vector3 fullOffset = new Vector3(flatOffset.x, 0, flatOffset.y);
        List<List<Vector3>> linePositions = GetPath(new Vector2Int(-moveDifference.x, moveDifference.y), flatOffset, -map.SampleHeight(fullOffset), !verticalFirst);
        List<Vector3> linePath = new List<Vector3>();
        foreach(List<Vector3> pointList in linePositions)
        {
            foreach(Vector3 point in pointList)
            {
                Vector3 scaledPathPos = point - fullOffset;
                scaledPathPos.x *= 1.0f / moveMarker.transform.lossyScale.x;
                scaledPathPos.z *= 1.0f / moveMarker.transform.lossyScale.z;
                linePath.Add(scaledPathPos);
            }
        }
        path.positionCount = linePath.Count;
        path.SetPositions(linePath.ToArray());
    }

    public List<List<Vector3>> GetPath(Vector2Int difference, Vector2Int startingPos, float verticalOffset, bool verticalFirst)
    {
        List<List<Vector3>> movementList = new List<List<Vector3>>();

        if (verticalFirst)
        {
            for (int y = 0; y < Mathf.Abs(difference.y); y++)
            {
                List<Vector3> positionList = new List<Vector3>();
                for (float ySlice = y; ySlice <= y + 1.05f; ySlice += 0.1f)
                {
                    float zPos = ySlice * Mathf.Sign(difference.y) + startingPos.y;
                    positionList.Add(new Vector3(startingPos.x, verticalOffset + map.SampleHeight(new Vector3(startingPos.x, 0, zPos)), zPos));
                }
                movementList.Add(positionList);
            }
            for (int x = 0; x < Mathf.Abs(difference.x); x++)
            {
                List<Vector3> positionList = new List<Vector3>();
                float zPos = difference.y + startingPos.y;
                for (float xSlice = x; xSlice <= x + 1.05f; xSlice += 0.1f)
                {
                    float xPos = xSlice * Mathf.Sign(difference.x) + startingPos.x;
                    positionList.Add(new Vector3(xPos, verticalOffset + map.SampleHeight(new Vector3(xPos, 0, zPos)), zPos));
                }
                movementList.Add(positionList);
            }
        }
        else
        {
            for(int x = 0; x < Mathf.Abs(difference.x); x++)
            {
                List<Vector3> positionList = new List<Vector3>();
                for (float xSlice = x; xSlice <= x + 1.05f; xSlice += 0.1f)
                {
                    float xPos = xSlice * Mathf.Sign(difference.x) + startingPos.x;
                    positionList.Add(new Vector3(xPos, verticalOffset + map.SampleHeight(new Vector3(xPos, 0, startingPos.y)), startingPos.y));
                }
                movementList.Add(positionList);
            }
            for (int y = 0; y < Mathf.Abs(difference.y); y++)
            {
                List<Vector3> positionList = new List<Vector3>();
                float xPos = difference.x + startingPos.x;
                for (float ySlice = y; ySlice <= y + 1.05f; ySlice += 0.1f)
                {
                    float zPos = ySlice * Mathf.Sign(difference.y) + startingPos.y;
                    positionList.Add(new Vector3(xPos, verticalOffset + map.SampleHeight(new Vector3(xPos, 0, zPos)), zPos));
                }
                movementList.Add(positionList);
            }
        }

        return movementList;
    }

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