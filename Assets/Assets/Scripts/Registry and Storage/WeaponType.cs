using System.Collections.Generic;
using UnityEngine;

public class WeaponType
{
    public string name { get; set; }
    //List of special effects acivated at certain ranges
    public List<WeaponStatsAtRange> ranges;
    public List<WeaponStatsAtRange> diagonalRanges;

    public WeaponType(string n)
    {
        name = n;
        ranges = new List<WeaponStatsAtRange>();
        diagonalRanges = new List<WeaponStatsAtRange>();
    }

    /// <summary>
    /// Gets the stats the weapon gives a given range from the attacker
    /// </summary>
    /// <param name="dist">The distance from the attacker to the target</param>
    /// <returns>The weapon stats at the given range or null if the weapon can't attack there</returns>
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