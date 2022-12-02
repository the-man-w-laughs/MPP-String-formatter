public class InvalidTemplateException : Exception
{
    public InvalidTemplateException()
    {
    }

    public InvalidTemplateException(string template)
        : base(String.Format("Invalid template: \"{0}\"", template))
    {
    }

}