namespace AdvancedAspNetCoreLab.DI;

public interface IMessageEnvelope<in T>
{
    string Describe(T value);
}

public sealed class MessageEnvelope<T> : IMessageEnvelope<T>
{
    public string Describe(T value)
    {
        return $"Envelope<{typeof(T).Name}> contains: {value}";
    }
}
