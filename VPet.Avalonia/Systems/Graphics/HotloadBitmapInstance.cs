using Avalonia.Media;
using VPet.Avalonia.Interfaces;

namespace VPet.Avalonia.Systems.Graphics;

public class HotLoadBitmapInstance : LazyBitmapInstance, IHotLoadable
{
    public HotLoadBitmapInstance(Func<IImage> factory) : base(factory)
    {
    }

    public void Unload()
    {
        OnDispose();
    }
}