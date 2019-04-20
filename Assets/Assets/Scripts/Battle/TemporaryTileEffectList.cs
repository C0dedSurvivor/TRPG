using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class TemporaryTileEffectList
{
    private List<Triple<Vector2Int, TileEffects, TemporaryEffectData>> tileEffects;

    public TemporaryTileEffectList()
    {
        tileEffects = new List<Triple<Vector2Int, TileEffects, TemporaryEffectData>>();
    }

    public List<TileEffects> GetTileEffects(Vector2Int pos, MoveTriggers trigger)
    {
        List<TileEffects> effect = new List<TileEffects>();
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

    public void StartOfTurn()
    {
        foreach(Triple<Vector2Int, TileEffects, TemporaryEffectData> data in tileEffects)
        {
            data.Third.StartOfTurn();
        }
    }

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
