using System.Globalization;

namespace CSharpCore;

public sealed class CourseValidationException : Exception
{
    public CourseValidationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

/// <summary>
/// Nullable annotations describe values that are allowed to be missing.
/// They help the compiler warn before a null reference reaches production.
/// </summary>
public sealed class OptionalProfile
{
    public OptionalProfile(string? nickname, int? age, DateTime? lastLoginAt)
    {
        Nickname = nickname;
        Age = age;
        LastLoginAt = lastLoginAt;
    }

    public string? Nickname { get; }

    public int? Age { get; }

    public DateTime? LastLoginAt { get; }
}

/// <summary>
/// Lesson 6 and 7: exceptions, custom domain errors, nullable values, and
/// null-safe access.
/// </summary>
public static class ExceptionAndNullabilityExamples
{
    public static int ParsePositiveNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new CourseValidationException("EMPTY_NUMBER", "A number is required.");
        }

        if (!int.TryParse(input, out int number))
        {
            throw new CourseValidationException("INVALID_NUMBER", "The value must be an integer.");
        }

        if (number <= 0)
        {
            throw new CourseValidationException("NON_POSITIVE_NUMBER", "The number must be positive.");
        }

        return number;
    }

    public static string TryParsePositiveNumber(string? input)
    {
        try
        {
            return ParsePositiveNumber(input).ToString(CultureInfo.InvariantCulture);
        }
        catch (CourseValidationException exception)
        {
            // Catching at an application boundary lets us turn a domain error
            // into a message or, later, an HTTP validation response.
            return $"{exception.Code}: {exception.Message}";
        }
    }

    public static string DivideWithCleanup(int numerator, int denominator, Action cleanup)
    {
        try
        {
            return (numerator / denominator).ToString(CultureInfo.InvariantCulture);
        }
        catch (DivideByZeroException)
        {
            return "Cannot divide by zero";
        }
        finally
        {
            // finally runs for both the success and exception paths. It is a
            // useful place for cleanup such as closing a resource.
            cleanup();
        }
    }

    public static string GetDisplayName(string? name)
    {
        // ?? supplies a fallback when the value on the left is null.
        return string.IsNullOrWhiteSpace(name) ? "Guest" : name.Trim();
    }

    public static string DescribeProfile(OptionalProfile profile)
    {
        // ?. accesses a member only when the nullable value has a value.
        string nickname = profile.Nickname ?? "Guest";
        string age = profile.Age?.ToString(CultureInfo.InvariantCulture) ?? "Unknown";
        string lastLogin = profile.LastLoginAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "Never";

        return $"{nickname} | Age: {age} | Last login: {lastLogin}";
    }

    public static void RequireProfile(OptionalProfile? profile)
    {
        // This built-in guard throws ArgumentNullException and tells the
        // nullable flow analyzer that profile is non-null after this point.
        ArgumentNullException.ThrowIfNull(profile);
    }
}
