namespace VPet.Avalonia.Systems.Actions.Triggers;

public class PetActionTrigger : IEquatable<PetActionTrigger>
{
    private static readonly List<Guid> UsedGuid = new ();

    private static Guid GetNewGuid()
    {
        while (true)
        {
            var guid = Guid.NewGuid();
            if(UsedGuid.Exists(a => guid == a))
                continue;

            UsedGuid.Add(guid);
            return guid;
        }
    }
    
    private readonly Guid _guid = GetNewGuid();
    
    public static PetActionTrigger Start { get; } = new ();
    
    public static PetActionTrigger Loop { get; } = new ();
    
    public static PetActionTrigger End { get; } = new ();
    
    public static PetActionTrigger Reset { get; } = new ();
    
    public bool Equals(PetActionTrigger? other)
    {
        return _guid == other?._guid;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj.GetType() == this.GetType() &&
               Equals((PetActionTrigger)obj);
    }

    public override int GetHashCode()
    {
        return _guid.GetHashCode();
    }
}