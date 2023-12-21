using System;
using System.IO;
using VPet.Avalonia.Interfaces;
using VPet.Avalonia.Modules;
using VPet.Avalonia.Options;
using VPet.Avalonia.Primitives;
using VPet.Avalonia.Providers.VPetSimulator.Options;
using VPet.Avalonia.Systems.Contents;

namespace VPet.Avalonia.Providers.VPetSimulator;

public class VPetSimCore : IModuleCore
{
    private string _rootPath;
    private VPetSimGfxService _gfxService;
    private VPetSimulatorAssetsProvider _assetsProvider;
    
    public void Initialise(string rootPath)
    {
        _rootPath = rootPath;
        _assetsProvider = new VPetSimulatorAssetsProvider(rootPath);
        _gfxService = new VPetSimGfxService();
        
        _gfxService.Init(_assetsProvider.GfxAssetFolderPath);
    }

    public void InitAssets(OptionsTable opts, Action<double> progressCallback)
    {
        var options = opts.As<GfxServiceInitOptions>();
        var appOptions = opts.As<ApplicationGfxOptions>();

        var cacheFolderPath = Path.Combine(appOptions.CacheFolderPath, "vpetsim");
        
        _gfxService.CreateSpriteSheetCacheIfNotExists(cacheFolderPath, uint.Parse(options.SpriteSize.ToString()), progressCallback);
        _gfxService.PreloadAllGfxSequencesWithHotLoadUnloadAndPreserveRam();
    }

    public AssetsProvider GetAssetsProvider()
    {
        return _assetsProvider;
    }

    public IGfxServiceInterface GetGfxService()
    {
        return _gfxService;
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

}