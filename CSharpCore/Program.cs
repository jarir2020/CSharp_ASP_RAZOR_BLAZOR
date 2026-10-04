using CSharpCore;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== PHASES 1-3: C# AND .NET FUNDAMENTALS ===");

Console.WriteLine("\n--- LESSON 1: Types, operators, control flow, and loops ---");
Console.WriteLine($"Types: {BasicsAndControlFlow.DescribeCommonTypes()}");

(int sum, int difference, int product, int quotient, int remainder) =
    BasicsAndControlFlow.CalculateArithmetic(10, 3);
Console.WriteLine($"Arithmetic: sum={sum}, difference={difference}, product={product}, quotient={quotient}, remainder={remainder}");
Console.WriteLine(BasicsAndControlFlow.DescribeResult(85));
Console.WriteLine($"For-loop sum from 1 to 5: {BasicsAndControlFlow.SumUsingForLoop(1, 5)}");
Console.WriteLine($"While-loop countdown: {string.Join(", ", BasicsAndControlFlow.Countdown(3))}");
Console.WriteLine($"Array range: {string.Join(", ", BasicsAndControlFlow.GetMiddleValues())}");

Console.WriteLine("\n--- LESSON 2: Collections, classes, and OOP ---");
(List<string> fruits, Dictionary<string, string> user, HashSet<int> uniqueNumbers) =
    DataStructuresAndOop.DemonstrateCollections();
Console.WriteLine($"List: {string.Join(", ", fruits)}");
string userName = user["name"];
string userRole = user["role"];
Console.WriteLine($"Dictionary user: {userName} ({userRole})");
Console.WriteLine($"HashSet: {string.Join(", ", uniqueNumbers.Order())}");
Console.WriteLine($"params total: {DataStructuresAndOop.CalculateTotal(10m, 20m, 30m):0.00}");

SimpleUser userObject = new("jarir", "jarir@example.com");
Console.WriteLine(userObject.GetInfo());
userObject.Deactivate();
Console.WriteLine(userObject.GetInfo());

Product product = new(1, "C# Course", 100m);
Console.WriteLine($"Product after tax: {product.PriceWithDefaultTax():0.00}");

PremiumAccount account = new("Jarir", new ConsoleMessageSender());
Console.WriteLine(account.Describe());
Console.WriteLine($"Monthly fee: {account.CalculateMonthlyFee():0.00}");
Console.WriteLine(account.NotifyOwner());
Console.WriteLine($"Generic first item: {GenericExamples.FirstItem(fruits)}");

Console.WriteLine("\n--- LESSON 3: Collections and collection interfaces ---");
Console.WriteLine($"Queue order: {string.Join(", ", CollectionExamples.ProcessQueue(new[] { "first", "second", "third" }))}");
Console.WriteLine($"Stack order: {string.Join(", ", CollectionExamples.UnwindStack(new[] { "first", "second", "third" }))}");
List<int> collection = new() { 1, 2 };
Console.WriteLine($"ICollection count after Add: {CollectionExamples.AddItem(collection, 3)}");
Console.WriteLine($"IList first item after replacement: {CollectionExamples.ReplaceFirst(new List<string> { "old", "second" }, "new")}");

Console.WriteLine("\n--- LESSON 4: LINQ ---");
IReadOnlyList<LearningUser> learningUsers = LinqExamples.CreateUsers();
Console.WriteLine($"Active users: {string.Join(", ", LinqExamples.GetActiveNames(learningUsers))}");
Console.WriteLine($"Flattened scores: {string.Join(", ", LinqExamples.FlattenScores(learningUsers))}");
Console.WriteLine($"Total score: {LinqExamples.GetTotalScore(learningUsers)}");
Console.WriteLine($"Distinct tags: {string.Join(", ", LinqExamples.GetDistinctTags(learningUsers))}");

Console.WriteLine("\n--- LESSON 5: Delegates, lambdas, and events ---");
PriceRule tenPercentDiscount = price => price * 0.90m;
Console.WriteLine($"Discounted price: {DelegateExamples.ApplyPriceRule(100m, tenPercentDiscount):0.00}");
IReadOnlyList<string> filteredNames = DelegateExamples.FilterNames(
    new[] { "Ava", "Bob", "Alex" },
    name => name.StartsWith("A", StringComparison.Ordinal));
Console.WriteLine($"Filtered names: {string.Join(", ", filteredNames)}");
string formattedName = DelegateExamples.FormatName("  jarir  ", name => name.ToUpperInvariant());
Console.WriteLine($"Formatted name: {formattedName}");
ProgressReporter reporter = new();
reporter.ProgressChanged += (_, eventArgs) => Console.WriteLine($"Progress event: {eventArgs.Percentage}%");
reporter.Report(50);

Console.WriteLine("\n--- LESSON 6 AND 7: Exceptions and nullable values ---");
string validNumber = ExceptionAndNullabilityExamples.TryParsePositiveNumber("42");
string invalidNumber = ExceptionAndNullabilityExamples.TryParsePositiveNumber("-1");
Console.WriteLine($"Valid number: {validNumber}");
Console.WriteLine($"Invalid number: {invalidNumber}");
int cleanupCalls = 0;
Console.WriteLine($"Safe division: {ExceptionAndNullabilityExamples.DivideWithCleanup(10, 0, () => cleanupCalls++)}");
Console.WriteLine($"Cleanup calls: {cleanupCalls}");
Console.WriteLine($"Optional display: {ExceptionAndNullabilityExamples.DescribeProfile(new OptionalProfile(null, null, null))}");

Console.WriteLine("\n--- LESSON 8: Async and parallel work ---");
IReadOnlyList<string> loadedLessons = await AsyncExamples.LoadLessonsAsync(new[] { "collections", "LINQ" });
Console.WriteLine($"Loaded lessons: {string.Join(", ", loadedLessons)}");
Console.WriteLine($"Async sum: {await AsyncExamples.SumAsync(new[] { 1, 2, 3 })}");
Console.WriteLine($"Parallel doubles: {string.Join(", ", await AsyncExamples.DoubleInParallelAsync(new[] { 1, 2, 3 }))}");

Console.WriteLine("\n--- LESSON 9 AND 10: .NET runtime, assemblies, memory, and CLI ---");
Console.WriteLine($"Framework: {DotNetFundamentals.GetFrameworkDescription()}");
Console.WriteLine($"Architecture: {DotNetFundamentals.GetProcessArchitecture()}");
Console.WriteLine($"Assembly: {DotNetFundamentals.GetAssemblyIdentity()}");
Console.WriteLine($"Base Class Library: {DotNetFundamentals.GetBaseClassLibraryIdentity()}");
Console.WriteLine($"Managed memory sample: {DotNetFundamentals.GetManagedMemoryBytes()} bytes");
Console.WriteLine($"Server GC enabled: {DotNetFundamentals.IsServerGarbageCollectionEnabled()}");
Console.WriteLine($"CLI commands: {string.Join(", ", DotNetFundamentals.GetCommonCliCommands())}");

Console.WriteLine("\n--- LESSON 12: Dependency injection and service lifetimes ---");
using (ServiceProvider provider = DependencyInjectionExamples.CreateProvider())
{
    using IServiceScope scope = provider.CreateScope();
    GreetingService greetingService = scope.ServiceProvider.GetRequiredService<GreetingService>();
    Console.WriteLine(greetingService.CreateGreeting("  Jarir  "));
}

LifetimeObservation lifetimeObservation = DependencyInjectionExamples.ObserveLifetimes();
Console.WriteLine($"Singleton shared: {lifetimeObservation.SingletonIsShared}");
Console.WriteLine($"Transient is new each time: {lifetimeObservation.TransientIsNewEachTime}");
Console.WriteLine($"Scoped shared in one scope: {lifetimeObservation.ScopedIsSharedWithinScope}");
Console.WriteLine($"Scoped new in another scope: {lifetimeObservation.ScopedIsNewForAnotherScope}");

Console.WriteLine("\nPHASES 1-3 COMPLETE");
