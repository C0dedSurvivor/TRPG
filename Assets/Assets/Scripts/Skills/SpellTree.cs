using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SpellTree
{
    public string name { get; set; }

    public List<Skill> spells;

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

    public void Add(Skill skill)
    {
        spells.Add(skill);
    }

    public SpellTree(string name, List<Skill> spells = null)
    {
        this.name = name;
        this.spells = spells ?? new List<Skill>();
    }
}