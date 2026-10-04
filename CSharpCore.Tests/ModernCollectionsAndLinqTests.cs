using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class ModernCollectionsAndLinqTests
{
    [Fact]
    public void Queue_is_first_in_first_out_and_stack_is_last_in_first_out()
    {
        string[] items = { "first", "second", "third" };

        Assert.Equal(new[] { "first", "second", "third" }, CollectionExamples.ProcessQueue(items));
        Assert.Equal(new[] { "third", "second", "first" }, CollectionExamples.UnwindStack(items));
    }

    [Fact]
    public void Collection_interfaces_expose_the_operations_they_promise()
    {
        List<int> numbers = new() { 1, 2 };
        List<string> names = new() { "old", "second" };

        Assert.Equal(3, CollectionExamples.AddItem(numbers, 3));
        Assert.Equal(3, CollectionExamples.CountItems(numbers));
        Assert.Equal("new", CollectionExamples.ReplaceFirst(names, "new"));
    }

    [Fact]
    public void Linq_filters_sorts_and_projects_active_users()
    {
        IReadOnlyList<LearningUser> users = LinqExamples.CreateUsers();

        Assert.Equal(new[] { "Alex", "Ava" }, LinqExamples.GetActiveNames(users));
        Assert.Equal(new[] { 90, 80, 70, 75, 95 }, LinqExamples.FlattenScores(users));
    }

    [Fact]
    public void Linq_supports_first_single_aggregate_group_and_join_operations()
    {
        IReadOnlyList<LearningUser> users = LinqExamples.CreateUsers();
        UserRole[] roles =
        {
            new(1, "Developer"),
            new(2, "Tester"),
            new(3, "Developer")
        };

        Assert.Equal("Ava", LinqExamples.GetFirstUser(users).Name);
        Assert.Equal("Alex", LinqExamples.FindFirstUser(users, "alex")?.Name);
        Assert.Equal("Bob", LinqExamples.GetOnlyUserById(users, 2).Name);
        Assert.Null(LinqExamples.FindOnlyUserById(users, 99));
        Assert.True(LinqExamples.AreAllUsersNamed(users));
        Assert.Equal(2, LinqExamples.CountActiveUsers(users));
        Assert.Equal(410, LinqExamples.GetTotalScore(users));
        Assert.Equal(new[] { "Alex", "Ava" }, LinqExamples.GroupNamesByTeam(users)["Web"]);
        Assert.Equal(
            new[] { "Alex: Developer", "Ava: Developer", "Bob: Tester" },
            LinqExamples.JoinUsersToRoles(users, roles));
        Assert.Equal(new[] { "APIs", "Blazor", "C#", "Testing" }, LinqExamples.GetDistinctTags(users));
        Assert.True(LinqExamples.HasTag(users, "blazor"));
        Assert.False(LinqExamples.HasTag(users, "Django"));
    }
}
