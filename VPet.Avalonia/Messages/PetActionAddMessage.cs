using VPet.Avalonia.Enums;

namespace VPet.Avalonia.Messages;

public class PetActionAddMessage
{
    public PetActivityState Action { get; set; }
    
    public string Group { get; set; }
    
    public string DisplayName { get; set; }
    
    public string SequenceId { get; set; }
}