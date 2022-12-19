using Core;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Tests.ExampleClasses;
using Xunit.Sdk;

namespace Tests;

public class UnitTest1
{
    [Fact]
    public void InvalidBracketSequence()
    {
        var user = new User("Nazar", "Masiukou");
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{} {", user));
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("}{", user));
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{} }", user));
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{}", user));
    }
    [Fact]
    public void EscapeCharacterTest()
    {
        var user = new User("Nazar", "Masiukou");
        Assert.Equal("{}",
        NewStringFormatter.Shared.Format("{{}}", user));
    }

    [Fact]
    public void SimpleStringFormatTest()
    {
        var user = new User("Nazar", "Masiukou");
        Assert.Equal($"Ну привет {{{user.FirstName}}} {user.LastName}!",
        NewStringFormatter.Shared.Format("Ну привет {{{FirstName}}} {LastName}!", user));
    }

    [Fact]
    public void InvalidIndexExpressionTest()
    {
        var user = new User("Nazar", "Masiukou");
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{user[a][b][c]}", user));
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{user[a]c]}", user));
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{user[a[c]}", user));
        Assert.ThrowsAny<Exception>(
            () => NewStringFormatter.Shared.Format("{abc[a]bc}", user));
    }

    [Fact]
    public void InnerDictionaryTest()
    {
        var dic = new MyDictionary();
        Assert.Equal($"{dic.alphabet["a"]["apple"]}",
        NewStringFormatter.Shared.Format("{alphabet[a][apple]}", dic)); ;
    }

    [Fact]
    public void ArrayTest()
    {
        var dic = new MyDictionary();
        Assert.Equal($"{dic.numbersArray[1]}",
        NewStringFormatter.Shared.Format("{numbersArray[1]}", dic)); ;
    }

    [Fact]
    public void ListTest()
    {
        var dic = new MyDictionary();
        Assert.Equal($"{dic.numbersList[1]}",
        NewStringFormatter.Shared.Format("{numbersList[1]}", dic)); ;
    }

    [Fact]
    public void DicTest()
    {
        var dic = new MyDictionary();
        Assert.Equal($"{dic.numbersDic[1]}",
        NewStringFormatter.Shared.Format("{numbersDic[1]}", dic)); ;
    }

}
