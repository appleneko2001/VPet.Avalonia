namespace VPet.Avalonia.Primitives;

public class OptionsTable
{
    private Dictionary<string, object?> _dictionary = new();
    
    public object? this[string id]
    {
        get => _dictionary.TryGetValue(id, out var r) ? r : null;
        set => _dictionary[id] = value;
    }

    public T As<T>() where T : OptionsTable, new()
    {
        return new T
        {
            _dictionary = _dictionary
        };
    }
}