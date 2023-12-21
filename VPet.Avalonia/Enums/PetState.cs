namespace VPet.Avalonia.Enums;

/// <summary>
/// Pet state (a combination with mood and health state)
/// </summary>
public enum PetState
{
    /// <summary>
    /// Sweetie is happy rn :)
    /// </summary>
    Happy,
    
    /// <summary>
    /// Pet is in "normal" state, which means mood and health are at middle.
    /// The alternative name of this value is "Nomal", which VPetSimulator creator wrote it with typo lol
    /// </summary>
    Normal,
    
    /// <summary>
    /// Pet is in "not good" condition (bad state). The alternative name of this value is "PoorCondition"
    /// </summary>
    Bad,
    
    /// <summary>
    /// Like it says, pet is in "ill" state. Time to take a pill!!!
    /// </summary>
    Ill
}

