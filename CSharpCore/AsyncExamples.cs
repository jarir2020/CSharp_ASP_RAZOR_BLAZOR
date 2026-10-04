namespace CSharpCore;

/// <summary>
/// Lesson 8: asynchronous work. These methods simulate I/O with Task.Delay so
/// the lesson stays local and does not call a real network service.
/// </summary>
public static class AsyncExamples
{
    public static async Task<string> LoadLessonAsync(
        string lessonName,
        CancellationToken cancellationToken = default)
    {
        // Delay represents waiting for I/O. During the wait, the thread can do
        // other work instead of blocking synchronously.
        await Task.Delay(5, cancellationToken);
        return $"Loaded: {lessonName}";
    }

    public static async Task<IReadOnlyList<string>> LoadLessonsAsync(
        IEnumerable<string> lessonNames,
        CancellationToken cancellationToken = default)
    {
        // Start all independent operations before awaiting them together.
        // WhenAll returns results in the same order as the task collection.
        Task<string>[] tasks = lessonNames
            .Select(name => LoadLessonAsync(name, cancellationToken))
            .ToArray();

        return await Task.WhenAll(tasks);
    }

    public static async Task<int> SumAsync(
        IEnumerable<int> numbers,
        CancellationToken cancellationToken = default)
    {
        int total = 0;

        foreach (int number in numbers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Yield gives the scheduler a chance to run other ready work. It
            // also makes the asynchronous nature visible in this small lesson.
            await Task.Yield();
            total += number;
        }

        return total;
    }

    public static async Task<IReadOnlyList<int>> DoubleInParallelAsync(
        IEnumerable<int> numbers,
        CancellationToken cancellationToken = default)
    {
        int[] input = numbers.ToArray();
        int[] doubled = new int[input.Length];

        // Parallel.For is for CPU-bound work that can safely run concurrently.
        // This is different from async I/O, where we normally await the I/O
        // operation instead of creating extra worker threads.
        await Task.Run(
            () => Parallel.For(
                0,
                input.Length,
                new ParallelOptions { CancellationToken = cancellationToken },
                index => doubled[index] = input[index] * 2),
            cancellationToken);

        return doubled;
    }
}
