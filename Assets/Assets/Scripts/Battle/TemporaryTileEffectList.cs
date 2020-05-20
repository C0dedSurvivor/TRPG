using System.Collections.Generic;
using UnityEngine;

class TemporaryTileEffectList
{
    private List<Triple<Vector2Int, TileType, TemporaryEffectData>> tileEffects;

    public TemporaryTileEffectList()
    {
        tileEffects = new List<Triple<Vector2Int, TileType, TemporaryEffectData>>();
    }

    /// <summary>
    /// Gets what effects are triggered by a given action on a given tile
    /// </summary>
    /// <param name="pos">The position of the affected tile</param>
    /// <param name="trigger">The action that might trigger an effect</param>
    /// <returns></returns>
    public List<TileType> GetTileType(Vector2Int pos, MoveTriggers trigger)
    {
        List<TileType> effect = new List<TileType>();
        for (int i = 0; i < tileEffects.Count; i++)
        {
            if (tileEffects[i].First.x == pos.x && tileEffects[i].First.y == pos.y
                && tileEffects[i].Third.Activatable() && tileEffects[i].Second.Contains(trigger))
            {
                effect.Add(tileEffects[i].Second);
                tileEffects[i].Third.Activated();
                if (tileEffects[i].Third.OutOfUses())
                {
                    tileEffects.RemoveAt(i);
                    i--;
                }
            }
        }
        return effect;
    }

    /// <summary>
    /// Activates the start of turn for each effect's data
    /// </summary>
    public void StartOfTurn()
    {
        foreach (Triple<Vector2Int, TileType, TemporaryEffectData> data in tileEffects)
        {
            data.Third.StartOfTurn();
        }
    }

    /// <summary>
    /// Activates the end of turn for each effect's data and removes those that can no longer trigger
    /// </summary>
    public void EndOfTurn()
    {
        for (int i = 0; i < tileEffects.Count; i++)
        {
            if (tileEffects[i].Third.EndOfTurn())
            {
                tileEffects.RemoveAt(i);
                i--;
            }
        }
    }
}
