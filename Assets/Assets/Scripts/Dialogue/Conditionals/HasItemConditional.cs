class HasItemConditional : ConditionalCheck
{
    private string itemName;
    private int amount;

    public HasItemConditional(string itemName, int amount = 1)
    {
        this.itemName = itemName;
        this.amount = amount;
    }

    public bool Evaluate()
    {
        return Inventory.GetItemAmount(itemName) >= amount;
    }
}
