namespace DotNetDelta;

/// <summary>
/// Defines the interface for an embed handler. An embed handler is just a class that defines how to compose,
/// invert, and transform two embeds.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEmbedHandler<T> where T : AttributeMap
{
    public string GetName();

    public T Compose(T a, T b, bool keepNull);
    public T Invert(T a, T b);
    public T? Transform(T a, T b, bool priority);

    public static (string, AttributeMap, AttributeMap) GetEmbedTypeAndData(
        AttributeMap a,
        AttributeMap b)
    {
        if (a == null || b == null)
        {
            throw new Exception("Embeds cannot both be null");
        }

        var embedType = a.Keys.First();
        if (b.Keys.First() != embedType)
        {
            throw new Exception("Embed types must match");
        }

        return (embedType, (AttributeMap)a[embedType], (AttributeMap)b[embedType]);
    }
}

public class DefaultEmbedHandler : IEmbedHandler<AttributeMap>
{
    private string _name;
    
    public DefaultEmbedHandler(string name)
    {
        _name = name;
    }


    public string GetName()
    {
        return _name;
    }


    public AttributeMap Compose(AttributeMap a, AttributeMap b, bool keepNull)
    {
        return new AttributeMap { { _name, AttributeMap.Compose(a, b, keepNull) } };
    }

    public AttributeMap Invert(AttributeMap a, AttributeMap b)
    {
        return new AttributeMap { { _name, AttributeMap.Invert(a, b) } };
    }

    public AttributeMap? Transform(AttributeMap a, AttributeMap b, bool priority)
    {
        return new AttributeMap { { _name, AttributeMap.Transform(a, b, priority) } };
    }
}