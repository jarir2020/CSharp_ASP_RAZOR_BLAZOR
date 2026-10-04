namespace BlazorLab.Models;

public sealed record BrowserSnapshot(
    string UserAgent,
    int Width,
    int Height);
