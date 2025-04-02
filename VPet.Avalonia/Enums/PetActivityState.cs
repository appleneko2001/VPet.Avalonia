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
    Speaking,
    Startup,
    Shutdown,
    Activity,
    StateUp,
    StateDown,
    StateThirsty,
    StateHunger,
    
    /// Those entries is still not ready, they are just exist for further supporting yet
    
    /// Birthday
    BirthDay,
    
    /// Not implemented 
    Drink,
    Eat,
    Gift,
    
    LevelUp,
    
    /// Have some variants, still thinking how to do it lol
    Dancing,
    
    /// why most assets entries have no standardised patterns
    /// somehow they just related with LPS descriptor, and somehow they uses folder names as assets entries info
    /// STHP -> (Startup, Happy)
    /// STNM -> (Startup, Normal)
    /// STUH -> (Startup, Poor)
    /// IDELH -> (Idle, Happy)
    /// IDELN -> (Idle, Normal)
    /// IDELPC -> (Idle, Poor)
    /// exist LPS try let module do parser info.lps
    NewYear,
    
    /// I assume pinch is pull face 
    PullFace,
    
    /// Have different loop sequences
    Think,
}