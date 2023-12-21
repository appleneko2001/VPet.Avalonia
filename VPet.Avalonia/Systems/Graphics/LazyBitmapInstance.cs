using Avalonia.Media;
using VPet.Avalonia.Extensions;

namespace VPet.Avalonia.Systems.Graphics;

public class LazyBitmapInstance : IBitmapInstance
{
    private IImage? _instance;
    private readonly Func<IImage> _factory;
    
    public LazyBitmapInstance(Func<IImage> factory)
    {
        _factory = factory;
    }
    
    public void Dispose()
    {
        OnDispose();
    }

    protected void OnDispose()
    {
        if(_instance is IDisposable disposable)
            disposable.Dispose();
        _instance = null;
    }

    public IImage Instance => _instance ??= _factory.Invoke();
    public int UsedBytes => (int)(_instance?.GetUsedBytes() ?? 0);
}