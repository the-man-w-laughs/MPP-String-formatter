public class InvalidIndexExpression : Exception
{
    public InvalidIndexExpression()
    {
    }

    public InvalidIndexExpression(string expression)
        : base(String.Format("Invalid index expression: \"{0}\"", expression))
    {
    }

}