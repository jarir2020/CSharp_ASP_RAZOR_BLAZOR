using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class BasicsAndControlFlowTests
{
    [Fact]
    public void Arithmetic_returns_expected_operations()
    {
        var result = BasicsAndControlFlow.CalculateArithmetic(10, 3);

        Assert.Equal(13, result.Sum);
        Assert.Equal(7, result.Difference);
        Assert.Equal(30, result.Product);
        Assert.Equal(3, result.Quotient);
        Assert.Equal(1, result.Remainder);
    }

    [Theory]
    [InlineData(95, Grade.A)]
    [InlineData(85, Grade.B)]
    [InlineData(75, Grade.C)]
    [InlineData(50, Grade.F)]
    public void Grade_selection_uses_score_ranges(int score, Grade expected)
    {
        Assert.Equal(expected, BasicsAndControlFlow.GetGrade(score));
    }

    [Fact]
    public void Loops_and_array_ranges_return_expected_values()
    {
        Assert.Equal(15, BasicsAndControlFlow.SumUsingForLoop(1, 5));
        Assert.Equal(new[] { 3, 2, 1 }, BasicsAndControlFlow.Countdown(3));
        Assert.Equal(new[] { 20, 30, 40 }, BasicsAndControlFlow.GetMiddleValues());
    }
}
