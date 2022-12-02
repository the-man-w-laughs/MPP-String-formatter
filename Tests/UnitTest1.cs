namespace Tests;

public class UnitTest1
{
    [Fact]
    public void SimpleStringFormatTest()
    {

        var user = new User("Nazar", "Masiukou");    
        Assert.Equal($"Ну привет {user.FirstName} {user.LastName}!",
         StringFormatter.Shared.Format("Ну привет {FirstName} {LastName}!",user));
    }
}