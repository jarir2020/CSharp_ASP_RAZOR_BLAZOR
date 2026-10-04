namespace CSharpCore;

/// <summary>
/// A small enum gives a name to a fixed set of related values.
/// Named values are easier to understand than passing around magic numbers.
/// </summary>
public enum Grade
{
    A,
    B,
    C,
    F
}

/// <summary>
/// Structs are value types. This one is intentionally small and immutable:
/// once coordinates are created, their X and Y values cannot be changed.
/// </summary>
public readonly struct Coordinates
{
    public Coordinates(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double X { get; }

    public double Y { get; }
}

/// <summary>
/// Lesson 1: variables, types, operators, conditions, loops, and arrays.
/// </summary>
public static class BasicsAndControlFlow
{
    public static (int Sum, int Difference, int Product, int Quotient, int Remainder)
        CalculateArithmetic(int left, int right)
    {
        if (right == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(right), "The divisor cannot be zero.");
        }

        return (
            Sum: left + right,
            Difference: left - right,
            Product: left * right,
            Quotient: left / right,
            Remainder: left % right);
    }

    public static string DescribeCommonTypes()
    {
        // Each variable has a type that describes the kind of value it holds.
        int userId = 42;
        decimal monthlyPrice = 19.99m;
        bool isActive = true;
        char planCode = 'P';
        string username = "jarir";

        return $"{userId}:{monthlyPrice:0.00}:{isActive}:{planCode}:{username}";
    }

    public static Grade GetGrade(int score)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "A score must be between 0 and 100.");
        }

        // The conditions are checked from top to bottom. The first matching
        // branch becomes the result, so the order matters.
        if (score >= 90)
        {
            return Grade.A;
        }

        if (score >= 80)
        {
            return Grade.B;
        }

        if (score >= 70)
        {
            return Grade.C;
        }

        return Grade.F;
    }

    public static string DescribeResult(int score)
    {
        Grade grade = GetGrade(score);

        // The conditional operator is a compact if/else expression.
        string status = score >= 70 ? "Passed" : "Failed";
        return $"Grade: {grade} ({status})";
    }

    public static int SumUsingForLoop(int start, int endInclusive)
    {
        int total = 0;

        // A for loop is useful when we know the counter and its boundaries.
        for (int number = start; number <= endInclusive; number++)
        {
            total += number;
        }

        return total;
    }

    public static int[] Countdown(int startingNumber)
    {
        if (startingNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingNumber));
        }

        int[] countdown = new int[startingNumber];
        int index = 0;
        int current = startingNumber;

        // A while loop repeats while its condition remains true.
        while (current > 0)
        {
            countdown[index] = current;
            index++;
            current--;
        }

        return countdown;
    }

    public static int[] GetMiddleValues()
    {
        int[] numbers = { 10, 20, 30, 40, 50 };

        // The range operator uses an inclusive start and exclusive end:
        // index 1..4 returns the values at indexes 1, 2, and 3.
        return numbers[1..4];
    }
}
