using Core;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

public class NewStringFormatter : IStringFormatter
{
    public static readonly NewStringFormatter Shared = new NewStringFormatter();
    private ConcurrentDictionary<string, Func<object, object>> _cache = new();

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

                    fieldName = fieldName.Trim();

                    var tuple = getPropertyNameAndIndexes(fieldName);

                    var propertyName = tuple.Item1;
                    var indexes = tuple.Item2;

                    var property = GetTargetProperty(propertyName, target);
                 
                    if (property != null)
                    {
                        if (indexes.Count != 0)
                        {
                            result.Append(GetObjectByIndex(property, indexes).ToString());
                        }
                        else
                        {
                            result.Append(property.ToString());
                        }                        
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

    private Tuple<string, List<string>?>? getPropertyNameAndIndexes(string template)
    {
        string propertyName;
        var indexes = new List<string>();

        // get property name 
        var openPos = template.IndexOf('[', 0);

        if (openPos != -1)
        {
            propertyName = template.Substring(0, openPos);
        }
        else
        {
            return Tuple.Create(template, indexes);
        }

        var closePos = template.IndexOf(']', openPos);

        if (closePos != -1)
        {
            indexes.Add(template.Substring(openPos + 1, closePos - openPos - 1));
        }
        else
        {
            throw new InvalidTemplateException(template);
        }

        var stop = false;

        while (!stop)
        {
            openPos = template.IndexOf('[', closePos);

            if (openPos == -1)
            {
                stop = true;
            }
            else
            {

                closePos = template.IndexOf(']', openPos);

                if (closePos == -1)
                {
                    throw new InvalidTemplateException(template);
                }
                else
                {
                    indexes.Add(template.Substring(openPos + 1, closePos - openPos - 1));
                }
            }
        }

        // chech if after a close bracket there is no characters
        if (closePos == template.Length - 1)
        {
            return Tuple.Create(propertyName, indexes);
        }
        else
            throw new InvalidTemplateException(template);
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

    private object? GetObjectByIndex(object container, List<string> indexes)
    {
        var strIndexes = indexes;

        object currList = container;

        // create listType to make conversion (listType)Array later...
        // IList interfase is implemented by all array - like structures
        // IList contains Index property, used to access values by indexes
        Type listType;

        foreach (var strIndex in strIndexes)
        {
            // create listType to make conversion (listType)Array later...
            // IList interfase is implemented by all array - like structures
            // IList contains Index property, used to access values by indexes
            listType = currList.GetType();

            // objIndex represents index in form of object 
            // in order to create lambda<<Func<object, object, object>> later

            object objIndex;

            // indexType to convert indexExpr to desired type

            Type indexType;

            int intIndex;

            int.TryParse(strIndex, out intIndex);

            // get index type 
            // create appropriate objIndex
            if (TypeChecker.IsDictionary(currList))
            {
                indexType = listType.GetGenericArguments()[0];

                if (indexType == typeof(string))
                {
                    objIndex = strIndex;
                }
                else
                {
                    objIndex = intIndex;
                }
            }
            else
            {
                indexType = typeof(int);

                objIndex = intIndex;
            }

            var arrayExpr = Expression.Parameter(typeof(object), "Array");

            var indexExpr = Expression.Parameter(typeof(object), "Index");

            // represents accessing to Item propetry
            // 
            Expression getValueByIndexExpr;

            if (listType.IsArray)
            {
                getValueByIndexExpr = Expression.ArrayAccess(Expression.Convert(arrayExpr, listType), Expression.Convert(indexExpr, indexType));
            }
            else
            {
                getValueByIndexExpr = Expression.Property(Expression.Convert(arrayExpr, listType), "Item", Expression.Convert(indexExpr, indexType));
            }

            var lambdaExpr = Expression.Lambda<Func<object, object, object>>(
             getValueByIndexExpr,
             new[] { arrayExpr, indexExpr });

            var lambda = lambdaExpr.Compile();

            currList = lambda(currList, objIndex);
        }

        var result = currList;
        return result;
    }

    private object? GetTargetProperty(string input, object target)
    {
        string propertyAccessorStr = target.GetType().ToString() + "." + input;

        if (!_cache.ContainsKey(propertyAccessorStr))
        {
            try
            {
                var param = Expression.Parameter(typeof(object), "target");

                var curObjParam = Expression.PropertyOrField(Expression.TypeAs(param, target.GetType()), input);

                var lambda = Expression.Lambda<Func<object, object>>(curObjParam, new[] { param });

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