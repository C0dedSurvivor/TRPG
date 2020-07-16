class ConditionalNot : ConditionalCheck
{
    private ConditionalCheck condition1;

    public ConditionalNot(ConditionalCheck condition1)
    {
        this.condition1 = condition1;
    }

    public bool Evaluate()
    {
        return !condition1.Evaluate();
    }
}
