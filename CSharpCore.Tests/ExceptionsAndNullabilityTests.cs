using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class ExceptionsAndNullabilityTests
{
    [Fact]
    public void Custom_exception_carries_a_domain_error_code()
    {
        CourseValidationException exception = Assert.Throws<CourseValidationException>(
            () => ExceptionAndNullabilityExamples.ParsePositiveNumber("-1"));

        Assert.Equal("NON_POSITIVE_NUMBER", exception.Code);
    }

    [Fact]
    public void Catching_a_custom_exception_can_create_a_user_facing_message()
    {
        Assert.Equal(
            "INVALID_NUMBER: The value must be an integer.",
            ExceptionAndNullabilityExamples.TryParsePositiveNumber("abc"));
    }

    [Fact]
    public void Finally_runs_after_both_success_and_failure()
    {
        int cleanupCalls = 0;

        string success = ExceptionAndNullabilityExamples.DivideWithCleanup(10, 2, () => cleanupCalls++);
        string failure = ExceptionAndNullabilityExamples.DivideWithCleanup(10, 0, () => cleanupCalls++);

        Assert.Equal("5", success);
        Assert.Equal("Cannot divide by zero", failure);
        Assert.Equal(2, cleanupCalls);
    }

    [Fact]
    public void Nullable_values_use_explicit_fallbacks()
    {
        OptionalProfile profile = new(null, null, null);

        Assert.Equal("Guest | Age: Unknown | Last login: Never", ExceptionAndNullabilityExamples.DescribeProfile(profile));
        Assert.Equal("Guest", ExceptionAndNullabilityExamples.GetDisplayName(null));
        Assert.Equal("Jarir", ExceptionAndNullabilityExamples.GetDisplayName(" Jarir "));
    }
}
