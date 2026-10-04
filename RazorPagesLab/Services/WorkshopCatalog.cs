using RazorPagesLab.Models;

namespace RazorPagesLab.Services;

public sealed class WorkshopCatalog
{
    private readonly object _sync = new();
    private readonly List<Workshop> _workshops = new()
    {
        new(
            1,
            "ASP.NET Core Request Pipeline",
            "Trace a request through routing, middleware, and a Razor Page.",
            DateTime.UtcNow.Date.AddDays(7),
            8),
        new(
            2,
            "Practical C# LINQ",
            "Transform collections with readable, composable queries.",
            DateTime.UtcNow.Date.AddDays(14),
            0)
    };

    public IReadOnlyList<Workshop> GetAll()
    {
        lock (_sync)
        {
            // Return a snapshot so a page renders a stable view of the list.
            return _workshops.ToArray();
        }
    }

    public Workshop? Find(int id)
    {
        lock (_sync)
        {
            return _workshops.FirstOrDefault(workshop => workshop.Id == id);
        }
    }

    public bool TryEnroll(int id)
    {
        lock (_sync)
        {
            int index = _workshops.FindIndex(workshop => workshop.Id == id);

            if (index < 0 || _workshops[index].SeatsRemaining == 0)
            {
                return false;
            }

            Workshop workshop = _workshops[index];
            _workshops[index] = workshop with
            {
                SeatsRemaining = workshop.SeatsRemaining - 1
            };

            return true;
        }
    }
}
