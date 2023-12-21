using VPet.Avalonia.Enums;

namespace VPet.Avalonia.Messages;

/// <summary>
/// This message object will trigger "PetBaseActionFlow" to change their states.
/// </summary>
public class TriggerBasicSequenceMessage
{
    public PetActivityState TargetState;
    public Action? PostSequenceTask;
}