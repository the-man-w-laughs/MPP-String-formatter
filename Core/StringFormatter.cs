using System.Reflection;
using System.Text;

public class StringFormatter : IStringFormatter
{
    public static readonly StringFormatter Shared = new StringFormatter();

    public string Format(string template, object target)
    {
        if (!CheckString(template))
        {
            throw new InvalidTemplateException(template);
        }

        Type objClass = target.GetType();
        FieldInfo fld;
        PropertyInfo prp;

        var result = new StringBuilder();
        var strLen = template.Length;
        var stop = false;
        var anchor = 0;
        int startOpen = 0;
        int openPos;
        int closePos;
        string fieldName;
        while (!stop)
        {
            openPos = template.IndexOf('{', startOpen);
            if (openPos == -1)
            {
                result.Append(template.Substring(anchor, strLen - anchor));
                stop = true;
            }
            else
            {
                // escaping characters
                if (template[openPos + 1] == '{')
                {
                    startOpen += 2;
                }
                else
                {                    
                    result.Append(template.Substring(anchor,openPos - anchor));
                    closePos = template.IndexOf('}', openPos);                    
                    startOpen = closePos;
                    anchor = closePos + 1;
                    fieldName = template.Substring(openPos + 1, closePos - openPos - 1);

                    prp = objClass.GetProperty(fieldName);                              
                    if (prp != null){
                        result.Append(prp.GetValue(target).ToString()); 
                        continue;
                    }
                    fld = objClass.GetField(fieldName);                              
                    if (fld != null){
                        result.Append(fld.GetValue(target).ToString()); 
                        continue;
                    }
                    throw new InvalidFieldNameException(fieldName);
                }
            }
        }
        return result.ToString();
    }

    private bool CheckString(string template)
    {
        var x = 0;
        foreach (var ch in template)
        {
            if (ch == '{')
            {
                x++;
            }
            if (ch == '}')
            {
                x--;
            }
            if (x < 0)
            {
                return false;
            }
        }
        return true;
    }
}