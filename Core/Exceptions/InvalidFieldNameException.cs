public class InvalidFieldNameException : Exception
{
    public InvalidFieldNameException()
    {
    }

    public InvalidFieldNameException(string fieldName)
        : base(String.Format("Invalid field name: \"{0}\"", fieldName))
    {
    }

}