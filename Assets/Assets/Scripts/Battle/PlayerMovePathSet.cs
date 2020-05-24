using System.Collections.Generic;
using UnityEngine;

public class PlayerMovePathSet
{
    public List<List<Vector2Int>> pathList;
    public int currentPathIndex;

    public PlayerMovePathSet(List<List<Vector2Int>> pathList)
    {
        currentPathIndex = 0;
        this.pathList = sortByPathComplexities(pathList);
    }

    public List<Vector2Int> GetPath()
    {
        return pathList[currentPathIndex];
    }

    public void NextPath()
    {
        currentPathIndex++;
        if (currentPathIndex == pathList.Count)
            currentPathIndex = 0;
    }

    private List<List<Vector2Int>> sortByPathComplexities(List<List<Vector2Int>> paths)
    {
        List<int> complexities = new List<int>();

        foreach(List<Vector2Int> path in paths)
        {
            //1 point of complexity per movement needed
            int complexity = path.Count;
            //1 point of complexity for each turn needed
            for(int i = 2; i < path.Count; i++)
            {
                if (Vector2.Distance(path[i - 2], path[i - 1]) != Vector2.Distance(path[i - 1], path[i]))
                    complexity++;
            }
            complexities.Add(complexity);
        }

        for (int i = 0; i < complexities.Count - 1; i++)
        {
            for (int j = 0; j < complexities.Count - i - 1; j++)
            {
                if (complexities[j] > complexities[j + 1])
                {
                    int temp = complexities[j];
                    complexities[j] = complexities[j + 1];
                    complexities[j + 1] = temp;

                    List<Vector2Int> tempPath = paths[j];
                    paths[j] = paths[j + 1];
                    paths[j + 1] = tempPath;
                }
            }
        }
        return paths;
    }
}