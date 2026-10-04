using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class DataStructuresAndOopTests
{
    [Fact]
    public void Collections_show_list_dictionary_and_set_behavior()
    {
        var result = DataStructuresAndOop.DemonstrateCollections();

        Assert.Equal(new[] { "apple", "banana", "cherry" }, result.Fruits);
        Assert.Equal("Alice", result.User["name"]);
        Assert.Equal(new[] { 1, 2, 3 }, result.UniqueNumbers.Order());
    }

    [Fact]
    public void User_encapsulates_activation_state()
    {
        SimpleUser user = new("jarir", "jarir@example.com");

        Assert.True(user.IsActive);
        user.Deactivate();

        Assert.False(user.IsActive);
        Assert.Contains("Inactive", user.GetInfo());
    }

    [Fact]
    public void Inheritance_and_composition_work_together()
    {
        PremiumAccount account = new("Jarir", new ConsoleMessageSender());

        Assert.Equal("Premium account owned by Jarir", account.Describe());
        Assert.Equal(9.99m, account.CalculateMonthlyFee());
        Assert.Equal("Message sent: Hello Jarir", account.NotifyOwner());
    }

    [Fact]
    public void Generic_method_preserves_the_item_type()
    {
        string first = GenericExamples.FirstItem(new List<string> { "C#", ".NET" });

        Assert.Equal("C#", first);
    }
}
