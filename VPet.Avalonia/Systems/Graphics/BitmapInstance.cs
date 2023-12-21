using Avalonia.Media;
using Avalonia.Media.Imaging;
using VPet.Avalonia.Extensions;

namespace VPet.Avalonia.Systems.Graphics;

public class BitmapInstance : IBitmapInstance
{
    private readonly Bitmap _bitmap;
    
    public BitmapInstance(Stream stream)
    {
        _bitmap = new Bitmap(stream);
    }
    
    public BitmapInstance(Bitmap bitmap)
    {
        _bitmap = bitmap;
    }
    
    public void Dispose()
    {
        _bitmap?.Dispose();
    }

    public IImage Instance => _bitmap;
    public int UsedBytes => (int)(_bitmap?.GetUsedBytes() ?? 0);
}