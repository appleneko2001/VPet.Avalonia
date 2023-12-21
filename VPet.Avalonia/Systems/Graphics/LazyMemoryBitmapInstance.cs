using Avalonia.Media;
using Avalonia.Media.Imaging;
using VPet.Avalonia.Extensions;
using VPet.Avalonia.Interfaces;

namespace VPet.Avalonia.Systems.Graphics;

public class LazyMemoryBitmapInstance : IBitmapInstance, IHotLoadable
{
    public IImage Instance => _instance ??= CreateInstancePrivate();
    
    public int UsedBytes => (int)(_memoryStream.Length + (_instance?.GetUsedBytes() ?? 0));

    private readonly MemoryStream _memoryStream;
    private IImage? _instance;
    
    /// <summary>
    /// Create a LazyMemoryBitmapInstance.
    /// </summary>
    /// <param name="stream">Input stream, which providing data for upload to memory from bitmap file.</param>
    public LazyMemoryBitmapInstance(Stream stream)
    {
        _memoryStream = new MemoryStream();
        stream.CopyTo(_memoryStream);
    }

    private IImage CreateInstancePrivate()
    {
        _memoryStream.Seek(0, SeekOrigin.Begin);
        var bitmap = new Bitmap(_memoryStream);

        return bitmap;
    }
    
    public void Dispose()
    {
        Unload();
        _memoryStream.Close();
    }
    
    public void Unload()
    {
        if(_instance is IDisposable disposable)
            disposable.Dispose();
        _instance = null;
    }
}