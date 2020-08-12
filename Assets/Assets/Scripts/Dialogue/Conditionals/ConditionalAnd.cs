class ConditionalAnd : ConditionalCheck
{
    private ConditionalCheck condition1;
    private ConditionalCheck condition2;

    public ConditionalAnd(ConditionalCheck condition1, ConditionalCheck condition2)
    {
        this.condition1 = condition1;
        this.condition2 = condition2;
    }

    public bool Evaluate()
    {
        return condition1.Evaluate() && condition2.Evaluate();
    }
}
