namespace DotNetDelta;

public class Utils
{
    public static bool DictionaryEquals(Dictionary<string, object>? a, Dictionary<string, object>? b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        
        // Check if either dictionary is null
        if (a == null || b == null)
            return false;
        
        if (!a.Any() && !b.Any())
        {
            return true;
        }
        
        if (a.Count != b.Count)
        {
            return false;
        }

        var comparer = EqualityComparer<object>.Default;
        return a.All(kvp => b.ContainsKey(kvp.Key) && comparer.Equals(kvp.Value, b[kvp.Key]));
    }
}