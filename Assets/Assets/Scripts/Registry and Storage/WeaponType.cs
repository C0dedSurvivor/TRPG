using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WeaponType
{
    string name;
    //List of special effects acivated at certain ranges
    public List<WeaponStatsAtRange> ranges;
    public List<WeaponStatsAtRange> diagonalRanges;
    public WeaponType(string n)
    {
        name = n;
        ranges = new List<WeaponStatsAtRange>();
        diagonalRanges = new List<WeaponStatsAtRange>();
    }

    public WeaponStatsAtRange GetStatsAtRange(Vector2Int dist)
    {
        int trueDistance = Mathf.Max(Mathf.Abs(dist.x), Mathf.Abs(dist.y));
        bool diagonal = Mathf.Abs(dist.x) == Mathf.Abs(dist.y);
        if (diagonal)
        {
            foreach (WeaponStatsAtRange r in diagonalRanges)
            {
                if (r.atDistance == trueDistance)
                    return r;
            }
        }
        else
        {
            foreach (WeaponStatsAtRange r in ranges)
            {
                if (r.atDistance == trueDistance)
                    return r;
            }
        }
        return null;
    }
}