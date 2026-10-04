using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class DelegatesLambdasAndEventsTests
{
    [Fact]
    public void Delegate_and_lambda_can_apply_a_business_rule()
    {
        PriceRule discount = price => price * 0.90m;

        Assert.Equal(90m, DelegateExamples.ApplyPriceRule(100m, discount));
        Assert.Equal("JARIR", DelegateExamples.FormatName(" jarir ", name => name.ToUpperInvariant()));
    }

    [Fact]
    public void Predicate_filters_and_Action_visits_each_name()
    {
        string[] names = { "Ava", "Bob", "Alex" };
        List<string> visited = new();

        Assert.Equal(new[] { "Ava", "Alex" }, DelegateExamples.FilterNames(names, name => name.StartsWith("A")));
        DelegateExamples.VisitNames(names, visited.Add);

        Assert.Equal(names, visited);
    }

    [Fact]
    public void Event_notifies_subscribers_when_progress_changes()
    {
        ProgressReporter reporter = new();
        List<int> percentages = new();
        reporter.ProgressChanged += (_, eventArgs) => percentages.Add(eventArgs.Percentage);

        reporter.Report(25);
        reporter.Report(100);

        Assert.Equal(new[] { 25, 100 }, percentages);
    }
}
