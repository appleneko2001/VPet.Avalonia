using VPet.Avalonia.Debugging;

namespace VPet.Avalonia.Systems;

/// <summary>
/// Internal use! A internal event communication bus, it allows broadcast and receive events between two or multiple unrelated classes.
/// </summary>
internal class EventBus
{
    internal static readonly EventBus Current = new ();
    
    private readonly Dictionary<Type, List<object>> Receivers = new ();
    
    internal void Listen<TObj>(Action<TObj> receiver)
    {
        var target = typeof(TObj);
        if (!Receivers.TryGetValue(target, out var receivers))
        {
            var list = new List<object>();
            receivers = list;
        }
        
        receivers.Add(receiver);
        Receivers[target] = receivers;
    }
    
    internal void Post<TObj>(TObj msg)
    {
        var target = typeof(TObj);
        if (!Receivers.TryGetValue(target, out var receivers))
        {
            this.WriteLine(MessageSeverity.Error, $"No receivers to handle message {target.FullName}");
            return;
        }

        foreach (Action<TObj> receiver in receivers)
        {
            receiver?.Invoke(msg);
        }
    }
}