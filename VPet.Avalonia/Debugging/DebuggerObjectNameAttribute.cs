namespace VPet.Avalonia.Debugging;

public class DebuggerObjectNameAttribute : Attribute
{
    public string Name { get; private set; }
    
    public DebuggerObjectNameAttribute(string name)
    {
        Name = name;
    }
}