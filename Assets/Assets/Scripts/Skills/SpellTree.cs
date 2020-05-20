using System.Collections.Generic;

public class SpellTree
{
    public string name { get; set; }

    public List<Skill> spells;

    /// <summary>
    /// Gets or sets the skill at the given index of the tree
    /// </summary>
    /// <param name="i">Index of the skill</param>
    /// <returns>Skill at the given index</returns>
    public Skill this[int i]
    {
        get
        {
            return spells[i];
        }
        set
        {
            spells[i] = value;
        }
    }

    public SpellTree(string name, List<Skill> spells = null)
    {
        this.name = name;
        this.spells = spells ?? new List<Skill>();
    }
}