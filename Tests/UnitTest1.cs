namespace Tests;

public class UnitTest1
{    
    [Fact]
    public void InvalidBracketSequence()
    {
        var user = new User("Nazar", "Masiukou");
        Assert.Throws<InvalidTemplateException>(
            () => StringFormatter.Shared.Format("{} {", user));
        Assert.Throws<InvalidTemplateException>(
            () => StringFormatter.Shared.Format("}{", user));
        Assert.Throws<InvalidTemplateException>(
            () => StringFormatter.Shared.Format("{} }", user));
    }
    [Fact]
    public void EscapeCharacterTest(){
        var user = new User("Nazar", "Masiukou");
        Assert.Equal("{}",
        StringFormatter.Shared.Format("{{}}", user));
    }

    [Fact]
    public void SimpleStringFormatTest()
    {
        var user = new User("Nazar", "Masiukou");
        Assert.Equal($"Ну привет {user.FirstName} {user.LastName}!",
        StringFormatter.Shared.Format("Ну привет {FirstName} {LastName}!", user));
    }
}