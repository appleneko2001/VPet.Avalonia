namespace VPet.Avalonia.Interfaces;

/// <summary>
/// Object with ability to unload until it acquired again.
/// </summary>
public interface IHotLoadable
{
    void Unload();
}