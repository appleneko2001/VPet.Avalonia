namespace VPet.Avalonia.Enums;

/// <summary>
/// The pet activity state
/// </summary>
// TODO: Requires refactoring work
public enum PetActivityState
{
    /// <summary>
    /// Just like it says, this key does nothing. Just a "placeholder" thing.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// On player starts dragging pet and moving. Better naming probably is "DragMoving"
    /// </summary>
    RaisedDynamic,
    
    /// <summary>
    /// On player dragging pet without move (holding). Better naming probably is "DragHold"
    /// </summary>
    RaisedStatic,
    
    /// <summary>
    /// On pet moving to where the pet decides to go
    /// </summary>
    Move,
    
    /// <summary>
    /// Common idle
    /// </summary>
    CommonIdle,
    TouchHead,
    TouchBody,
    
    /// <summary>
    /// Rare idle sequences
    /// </summary>
    Idle,
    Idle1,
    Idle2,
    Sleep,
    Say,
    Startup,
    Shutdown,
    Work,
    SwitchUp,
    SwitchDown,
    SwitchThirsty,
    SwitchHunger
}