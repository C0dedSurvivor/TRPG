using System.Collections.Generic;

/// <summary>
/// An item that has an effect when used during battle
/// </summary>
public class BattleItemBase : ItemBase
{
    public TargettingType targetType;

    //Whether this item can be used outside of battle
    public bool usableOutOfBattle;

    public List<SkillPartBase> partList = new List<SkillPartBase>();

    public BattleItemBase(TargettingType targetType, bool outOfBattleUse, List<SkillPartBase> effects, int maxStack, int sellPrice, string flavorText = "") : base(maxStack, sellPrice, flavorText)
    {
        this.targetType = targetType;
        usableOutOfBattle = outOfBattleUse;
        if (effects != null)
        {
            foreach (SkillPartBase part in effects)
            {
                partList.Add(part);
            }
        }
    }
}