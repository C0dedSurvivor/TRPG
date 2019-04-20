using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Some weapons have different stats or effects based on how far away the pawn being hit is
/// </summary>
public class WeaponStatsAtRange
{
    public DamageType damageType;
    //At what distance the wepon gets these effects
    public int atDistance;
    //Does it heal
    public bool heals;
    //Is the weapon ranged. If it is, attack spaces don't depend on whether the previous space is passable
    public bool ranged;
    //Damage multiplier
    public float damageMult;
    public WeaponStatsAtRange(int distance, bool ranged, DamageType type, bool heals = false, float multiplier = 1)
    {
        atDistance = distance;
        this.ranged = ranged;
        damageType = type;
        this.heals = heals;
        damageMult = multiplier;
    }
}