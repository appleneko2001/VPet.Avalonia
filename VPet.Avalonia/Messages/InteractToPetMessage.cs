using VPet.Avalonia.Enums;

namespace VPet.Avalonia.Messages;

public class InteractToPetMessage
{
    public InteractPetActionKind Action;
    public object? Params;
}