namespace CSharpCore;

/// <summary>
/// A custom delegate names a method signature that can be passed around.
/// </summary>
public delegate decimal PriceRule(decimal originalPrice);

/// <summary>
/// Lesson 5: delegates, lambdas, Action, Func, Predicate, and events.
/// </summary>
public static class DelegateExamples
{
    public static decimal ApplyPriceRule(decimal price, PriceRule rule)
    {
        return rule(price);
    }

    public static IReadOnlyList<string> FilterNames(
        IEnumerable<string> names,
        Predicate<string> predicate)
    {
        // Predicate<T> represents a function that receives T and returns bool.
        return names.Where(name => predicate(name)).ToList();
    }

    public static void VisitNames(IEnumerable<string> names, Action<string> visitor)
    {
        // Action<T> represents a function that receives T and returns no value.
        foreach (string name in names)
        {
            visitor(name);
        }
    }

    public static string FormatName(string name, Func<string, string>? formatter = null)
    {
        // Func<T, TResult> represents a function that receives T and returns TResult.
        Func<string, string> selectedFormatter = formatter ?? (value => value.Trim());
        return selectedFormatter(name);
    }
}

public sealed class ProgressChangedEventArgs : EventArgs
{
    public ProgressChangedEventArgs(int percentage)
    {
        Percentage = percentage;
    }

    public int Percentage { get; }
}

public sealed class ProgressReporter
{
    // Subscribers can listen without being allowed to raise the event themselves.
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

    public void Report(int percentage)
    {
        if (percentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage));
        }

        // ?. invokes the event only when at least one subscriber exists.
        ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(percentage));
    }
}
