using Avalonia.Media;

namespace VPet.Avalonia.Systems.Graphics;

public interface IBitmapInstance : IDisposable
{
    public IImage Instance { get; }
    
    public int UsedBytes { get; }
}