/// <summary>
/// A basic material
/// Can only be collected and sold
/// </summary>
public class ItemBase
{
    public string name { get; set; }
    //Max amount the player can hold
    int maxStack;
    //How much a single item sells for
    int sellAmount;
    string flavorText;

    public int MaxStack
    {
        get
        {
            return maxStack;
        }
    }

    public int SellAmount
    {
        get
        {
            return sellAmount;
        }
    }

    public string FlavorText
    {
        get
        {
            return flavorText;
        }
    }

    public ItemBase(int maxstack, int sell, string flavor = "")
    {
        maxStack = maxstack;
        sellAmount = sell;
        flavorText = flavor;
    }
}