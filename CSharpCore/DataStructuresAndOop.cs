namespace CSharpCore;

/// <summary>
/// Lesson 2: collections, methods, properties, constructors, and OOP.
/// </summary>
public static class DataStructuresAndOop
{
    public static (List<string> Fruits, Dictionary<string, string> User, HashSet<int> UniqueNumbers)
        DemonstrateCollections()
    {
        // List<T> is an ordered, changeable collection.
        List<string> fruits = new() { "apple", "banana" };
        fruits.Add("cherry");

        // Dictionary<TKey, TValue> stores values by a key.
        Dictionary<string, string> user = new()
        {
            ["name"] = "Alice",
            ["role"] = "Developer"
        };
        user["email"] = "alice@example.com";

        // HashSet<T> keeps only unique values. Adding a duplicate has no effect.
        HashSet<int> uniqueNumbers = new() { 1, 2, 2, 3, 3, 3 };

        return (fruits, user, uniqueNumbers);
    }

    public static decimal CalculateTotal(params decimal[] amounts)
    {
        // `params` lets callers pass zero or more decimal arguments. Inside the
        // method, C# exposes them as a normal array.
        decimal total = 0m;

        foreach (decimal amount in amounts)
        {
            total += amount;
        }

        return total;
    }
}

/// <summary>
/// A class is a reference type used to model an object with data and behavior.
/// </summary>
public sealed class SimpleUser
{
    private readonly string _email;

    public SimpleUser(string username, string email)
    {
        // `this` makes it explicit that the property belongs to this object.
        this.Username = username;
        this._email = email;
        this.IsActive = true;
    }

    public string Username { get; }

    public string Email => _email;

    // Private set means callers can read the state but only this class can
    // change it. This is a simple example of encapsulation.
    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public string GetInfo()
    {
        string status = IsActive ? "Active" : "Inactive";
        return $"User({Username}, {Email}) - {status}";
    }
}

public sealed class Product
{
    // `const` is a compile-time constant shared by every Product instance.
    public const decimal DefaultTaxRate = 0.05m;

    // `static readonly` is assigned once at runtime and then cannot be changed.
    public static readonly string Category = "Learning Example";

    public Product(int id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public int Id { get; }

    public string Name { get; }

    public decimal Price { get; }

    public decimal PriceWithDefaultTax()
    {
        return Price + (Price * DefaultTaxRate);
    }
}

/// <summary>
/// An interface describes a capability. Classes can implement the same
/// interface in different ways, which supports polymorphism.
/// </summary>
public interface IMessageSender
{
    string Send(string message);
}

public sealed class ConsoleMessageSender : IMessageSender
{
    public string Send(string message)
    {
        return $"Message sent: {message}";
    }
}

/// <summary>
/// An abstract class shares common state and behavior while requiring derived
/// classes to provide their own fee calculation.
/// </summary>
public abstract class Account
{
    protected Account(string owner)
    {
        Owner = owner;
    }

    public string Owner { get; }

    // Derived classes may replace this behavior with `override`.
    public virtual string Describe()
    {
        return $"Account owned by {Owner}";
    }

    public abstract decimal CalculateMonthlyFee();
}

public sealed class PremiumAccount : Account
{
    private readonly IMessageSender _messageSender;

    public PremiumAccount(string owner, IMessageSender messageSender)
        : base(owner)
    {
        // Receiving a dependency through the constructor is composition. The
        // account uses a sender but does not need to know its concrete class.
        _messageSender = messageSender;
    }

    public override string Describe()
    {
        return $"Premium account owned by {Owner}";
    }

    public override decimal CalculateMonthlyFee()
    {
        return 9.99m;
    }

    public string NotifyOwner()
    {
        return _messageSender.Send($"Hello {Owner}");
    }
}

/// <summary>
/// A generic method works with a type chosen by the caller while preserving
/// compile-time type safety.
/// </summary>
public static class GenericExamples
{
    public static T FirstItem<T>(IReadOnlyList<T> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("The collection cannot be empty.", nameof(items));
        }

        return items[0];
    }
}
