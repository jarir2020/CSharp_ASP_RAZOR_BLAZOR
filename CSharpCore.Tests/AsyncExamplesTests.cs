using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class AsyncExamplesTests
{
    [Fact]
    public async Task WhenAll_loads_independent_lessons_in_input_order()
    {
        IReadOnlyList<string> result = await AsyncExamples.LoadLessonsAsync(new[] { "collections", "LINQ" });

        Assert.Equal(new[] { "Loaded: collections", "Loaded: LINQ" }, result);
    }

    [Fact]
    public async Task Async_sum_returns_the_total_without_blocking_the_caller()
    {
        int result = await AsyncExamples.SumAsync(new[] { 1, 2, 3, 4 });

        Assert.Equal(10, result);
    }

    [Fact]
    public async Task Parallel_cpu_work_returns_each_doubled_value()
    {
        IReadOnlyList<int> result = await AsyncExamples.DoubleInParallelAsync(new[] { 1, 2, 3, 4 });

        Assert.Equal(new[] { 2, 4, 6, 8 }, result);
    }

    [Fact]
    public async Task Cancellation_token_stops_async_work()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => AsyncExamples.LoadLessonAsync("cancelled", cancellation.Token));
    }
}
