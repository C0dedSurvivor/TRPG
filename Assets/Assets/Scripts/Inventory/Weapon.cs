using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon {

	public int strength;
    public int defense;
	//sword, lance, axe, fists, shield, tome, bow
	public int weaponType;
	public int critChanceMod;

	public Weapon(int wT)
    {
        weaponType = wT;
        strength = 2;
        defense = 2;
        critChanceMod = 0; 
    }
    public Weapon(int wT, int str, int def, int crit)
    {
        weaponType = wT;
        strength = str;
        defense = def;
        critChanceMod = crit;
    }
}