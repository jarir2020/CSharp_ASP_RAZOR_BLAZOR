namespace CSharpCore;

/// <summary>
/// A small model used by the LINQ examples. Keeping the data in a class makes
/// the query operations look similar to filtering rows from a database.
/// </summary>
public sealed class LearningUser
{
    public LearningUser(
        int id,
        string name,
        bool isActive,
        string team,
        IEnumerable<int> scores,
        IEnumerable<string> tags)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
        Team = team;
        Scores = scores.ToArray();
        Tags = tags.ToArray();
    }

    public int Id { get; }

    public string Name { get; }

    public bool IsActive { get; }

    public string Team { get; }

    public IReadOnlyList<int> Scores { get; }

    public IReadOnlyList<string> Tags { get; }
}

public sealed class UserRole
{
    public UserRole(int userId, string role)
    {
        UserId = userId;
        Role = role;
    }

    public int UserId { get; }

    public string Role { get; }
}

/// <summary>
/// Lesson 3: collections and the collection interfaces used by .NET APIs.
/// </summary>
public static class CollectionExamples
{
    public static IReadOnlyList<string> ProcessQueue(IEnumerable<string> items)
    {
        // Queue<T> is first-in, first-out (FIFO). The first item added is the
        // first item returned by Dequeue().
        Queue<string> queue = new(items);
        List<string> processed = new();

        while (queue.Count > 0)
        {
            processed.Add(queue.Dequeue());
        }

        return processed;
    }

    public static IReadOnlyList<string> UnwindStack(IEnumerable<string> items)
    {
        // Stack<T> is last-in, first-out (LIFO). The most recent Push() is the
        // first item returned by Pop().
        Stack<string> stack = new();

        foreach (string item in items)
        {
            stack.Push(item);
        }

        List<string> unwound = new();
        while (stack.Count > 0)
        {
            unwound.Add(stack.Pop());
        }

        return unwound;
    }

    public static int CountItems(IEnumerable<int> items)
    {
        // IEnumerable<T> promises that callers can enumerate the values. It
        // does not promise that the values are stored in a particular type.
        return items.Count();
    }

    public static int AddItem(ICollection<int> items, int item)
    {
        // ICollection<T> promises collection operations such as Add and Count.
        // The caller can pass a List<T>, HashSet<T>, or another implementation.
        items.Add(item);
        return items.Count;
    }

    public static string ReplaceFirst(IList<string> items, string replacement)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("The list cannot be empty.", nameof(items));
        }

        // IList<T> adds index-based access on top of the collection behavior.
        items[0] = replacement;
        return items[0];
    }
}

/// <summary>
/// Lesson 4: LINQ query operators. LINQ works over IEnumerable<T>, so the
/// same style can be used with arrays, lists, and later with EF Core queries.
/// </summary>
public static class LinqExamples
{
    public static IReadOnlyList<LearningUser> CreateUsers()
    {
        return new List<LearningUser>
        {
            new(1, "Ava", true, "Web", new[] { 90, 80 }, new[] { "C#", "APIs" }),
            new(2, "Bob", false, "QA", new[] { 70, 75 }, new[] { "C#", "Testing" }),
            new(3, "Alex", true, "Web", new[] { 95 }, new[] { "Blazor" })
        };
    }

    public static IReadOnlyList<string> GetActiveNames(IEnumerable<LearningUser> users)
    {
        // Where filters objects, OrderBy and ThenBy sort them, and Select
        // projects each object into the value the caller actually needs.
        return users
            .Where(user => user.IsActive)
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .Select(user => user.Name)
            .ToList();
    }

    public static IReadOnlyList<int> FlattenScores(IEnumerable<LearningUser> users)
    {
        // SelectMany flattens many inner collections into one sequence.
        return users
            .SelectMany(user => user.Scores)
            .ToList();
    }

    public static LearningUser GetFirstUser(IEnumerable<LearningUser> users)
    {
        // First expresses that at least one item is required. It throws if the
        // sequence is empty, so use FirstOrDefault when empty is acceptable.
        return users.First();
    }

    public static LearningUser? FindFirstUser(IEnumerable<LearningUser> users, string name)
    {
        // FirstOrDefault returns null when no reference-type item matches.
        return users.FirstOrDefault(user =>
            string.Equals(user.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public static LearningUser GetOnlyUserById(IEnumerable<LearningUser> users, int id)
    {
        // Single documents that exactly one item must match. It throws when
        // there are zero matches or duplicate matches.
        return users.Single(user => user.Id == id);
    }

    public static LearningUser? FindOnlyUserById(IEnumerable<LearningUser> users, int id)
    {
        // SingleOrDefault allows zero or one match but still detects duplicates.
        return users.SingleOrDefault(user => user.Id == id);
    }

    public static bool AreAllUsersNamed(IEnumerable<LearningUser> users)
    {
        return users.All(user => !string.IsNullOrWhiteSpace(user.Name));
    }

    public static int CountActiveUsers(IEnumerable<LearningUser> users)
    {
        return users.Count(user => user.IsActive);
    }

    public static int GetTotalScore(IEnumerable<LearningUser> users)
    {
        return users
            .SelectMany(user => user.Scores)
            .Sum();
    }

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> GroupNamesByTeam(
        IEnumerable<LearningUser> users)
    {
        // GroupBy creates one group per team. ToDictionary materializes those
        // groups into a lookup that other application code can use.
        return users
            .GroupBy(user => user.Team)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(user => user.Name)
                    .OrderBy(name => name)
                    .ToList());
    }

    public static IReadOnlyList<string> JoinUsersToRoles(
        IEnumerable<LearningUser> users,
        IEnumerable<UserRole> roles)
    {
        // Join matches two sequences through related keys, like a simple SQL
        // INNER JOIN on users.Id = roles.UserId.
        return users
            .Join(
                roles,
                user => user.Id,
                role => role.UserId,
                (user, role) => $"{user.Name}: {role.Role}")
            .OrderBy(description => description)
            .ToList();
    }

    public static IReadOnlyList<string> GetDistinctTags(IEnumerable<LearningUser> users)
    {
        return users
            .SelectMany(user => user.Tags)
            .Distinct()
            .OrderBy(tag => tag)
            .ToList();
    }

    public static bool HasTag(IEnumerable<LearningUser> users, string tag)
    {
        // Contains answers a membership question without manually writing a loop.
        return users
            .SelectMany(user => user.Tags)
            .Contains(tag, StringComparer.OrdinalIgnoreCase);
    }
}
