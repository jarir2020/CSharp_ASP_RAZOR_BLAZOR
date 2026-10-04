namespace RazorPagesLab.Models;

// A small immutable model keeps the Razor examples focused on page rendering.
public sealed record Workshop(
    int Id,
    string Title,
    string Description,
    DateTime ScheduledFor,
    int SeatsRemaining);
