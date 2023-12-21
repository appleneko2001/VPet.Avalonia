namespace VPet.Avalonia.Mutables;

public interface IMutableCollection
{
    internal event EventHandler? UpdateRequested;

    internal void TryUpdate();
}
