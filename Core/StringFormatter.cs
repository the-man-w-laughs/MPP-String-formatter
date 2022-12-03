using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

public class StringFormatter : IStringFormatter
{
    public static readonly StringFormatter Shared = new StringFormatter();
    private ConcurrentDictionary<string, Func<object, string>> _cache = new();

    public string Format(string template, object target)
    {
        if (!CheckString(template))
        {
            throw new InvalidTemplateException(template);
        }
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
                    result.Append(template.Substring(anchor, openPos - anchor));

                    closePos = template.IndexOf('}', openPos);
                    startOpen = closePos;
                    anchor = closePos + 1;

                    fieldName = template.Substring(openPos + 1, closePos - openPos - 1);
                    var fieldStr = GetTargetStringProperty(fieldName, target);

                    if (fieldStr != null)
                    {
                        result.Append(fieldStr);
                    }
                    else
                    {
                        throw new InvalidFieldNameException(fieldName);
                    }
                }
            }
        }

        // escaping characters
        stop = false;
        startOpen = 0;
        while (!stop)
        {
            openPos = result.ToString().IndexOf('{', startOpen);
            if (openPos != -1)
            {
                result.Remove(openPos, 1);
                startOpen = openPos + 1;
            }
            else
            {
                stop = true;
            }
        }

        stop = false;
        var startClose = 0;
        while (!stop)
        {
            closePos = result.ToString().IndexOf('}', startClose);
            if (closePos != -1)
            {
                result.Remove(closePos, 1);
                startClose = closePos + 1;
            }
            else
            {
                stop = true;
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
        if (x == 0)
            return true;
        else
            return false;
    }

    private string? GetTargetStringProperty(string input, object target)
    {
        string propertyAccessorStr = target.GetType().ToString() + "." + input;

        if (!_cache.ContainsKey(propertyAccessorStr))
        {
            try
            {
                var param = Expression.Parameter(typeof(object), "target");

                var curObjParam = Expression.PropertyOrField(Expression.TypeAs(param, target.GetType()), input);

                var b = Expression.Call(curObjParam, "ToString", null, null);

                var lambda = Expression.Lambda<Func<object, string>>(b, new[] { param });

                var func = lambda.Compile();

                _cache.TryAdd(propertyAccessorStr, func);

                return func(target);

            }
            catch
            {

                return null;
            }
            
        }
       return _cache[propertyAccessorStr](target);
    }
}