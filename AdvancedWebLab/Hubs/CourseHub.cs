using Microsoft.AspNetCore.SignalR;

namespace AdvancedWebLab.Hubs;

public sealed class CourseHub : Hub
{
    // A hub method can broadcast a server-approved event to every connected
    // client. Authorization should be added before exposing this publicly.
    public Task Announce(string message)
    {
        string safeMessage = message.Trim();
        return Clients.All.SendAsync("CourseAnnouncement", safeMessage);
    }
}
