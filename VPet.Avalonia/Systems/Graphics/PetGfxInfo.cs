using VPet.Avalonia.Enums;

namespace VPet.Avalonia.Systems.Graphics;

public struct PetGfxInfo
{
    public string Name { get; set; }
    public PetActivityState? Activity { get; set; }
    public PetState? State { get; set; }
    public GfxAnimationType Transition { get; set; }
    
    public IReadOnlyList<string> Tags { get; set; }
    
    public int PathHashCode { get; set; }

    public bool IsSingle => Transition == GfxAnimationType.Single;

    public override int GetHashCode()
    {
        /*
         *    public override int GetHashCode()
    {
        return (Name?.GetHashCode() ?? 0) ^ 63 +
               (Activity?.GetHashCode() ?? 0) ^ 63 +
               (State?.GetHashCode() ?? 0) ^ 63 + PathHashCode + IsSingle.GetHashCode();
    }

         * 
         */
        return HashCode.Combine(Name.GetHashCode(), Activity.GetHashCode(), State.GetHashCode(),
            Transition.GetHashCode(), Tags.GetHashCode(), PathHashCode, IsSingle.GetHashCode());
    }

    public override string ToString()
    {
        return $"{Name} {Activity} {State} {Transition}, Tags: {string.Join(',', Tags)}";
    }
}