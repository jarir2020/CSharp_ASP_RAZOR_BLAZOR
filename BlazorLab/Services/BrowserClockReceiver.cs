using Microsoft.JSInterop;

namespace BlazorLab.Services;

public sealed class BrowserClockReceiver
{
    public event Action<string>? TimeReceived;

    [JSInvokable]
    public Task ReceiveBrowserTime(string currentTime)
    {
        // JavaScript calls this instance method through DotNetObjectReference.
        TimeReceived?.Invoke(currentTime);
        return Task.CompletedTask;
    }
}
