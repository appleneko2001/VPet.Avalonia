using VPet.Avalonia.Systems.Actions;

namespace VPet.Avalonia.Messages;

public class TryLetPetDoActionMessage
{
    public IPetAction Action = null!;
}