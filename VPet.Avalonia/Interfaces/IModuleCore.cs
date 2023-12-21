using VPet.Avalonia.Modules;
using VPet.Avalonia.Primitives;
using VPet.Avalonia.Systems.Contents;

namespace VPet.Avalonia.Interfaces;

/// <summary>
/// A interface used for providing pet animations and features to logic and gfx service.
/// </summary>
public interface IModuleCore : IDisposable
{
    void Initialise(string rootPath);

    void InitAssets(OptionsTable opts, Action<double> progressCallback);

    AssetsProvider GetAssetsProvider();
    
    IGfxServiceInterface GetGfxService();
}