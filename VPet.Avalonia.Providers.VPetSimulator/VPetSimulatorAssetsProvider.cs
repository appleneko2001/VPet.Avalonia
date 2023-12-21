using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using VPet.Avalonia.Extensions;
using VPet.Avalonia.Services;
using VPet.Avalonia.Systems.Contents;

namespace VPet.Avalonia.Providers.VPetSimulator;

/// <summary>
/// VPetSimulator assets provider. Please install https://store.steampowered.com/app/1920960/VPetSimulator/
/// and copy folder "mod" to folder "VPetSimulator" (case-insensitive, multiple folders with same name is not allowed.) 
/// </summary>
public class VPetSimulatorAssetsProvider : AssetsProvider
{
    private const string AssetsFolderName = "VPetSimulator";

    /// <summary>
    /// The main working directory for the module.
    /// </summary>
    internal readonly string RootFolderPath;
    
    /// <summary>
    /// The VPetSimulator core assets folder path.
    /// </summary>
    internal readonly string CoreAssetFolderPath;
    
    /// <summary>
    /// The VPetSimulator GFX assets folder path.
    /// </summary>
    internal readonly string GfxAssetFolderPath;
    
    internal VPetSimulatorAssetsProvider(string rootPath)
    {
        var assetRootPath = rootPath.CreateFolderThroughPath(AssetsFolderName);
        string modFolderPath;
        
        try
        {
            modFolderPath = assetRootPath.GetPath("mod");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException("Assets dependencies is not found. Please re-download the game. " +
                                                 $"Be make sure the folder \"mod\" is exist in folder \"{AssetsFolderName}\" and not empty.");
        }
        
        RootFolderPath = assetRootPath;
        CoreAssetFolderPath = modFolderPath.GetPath("0000_core");
        GfxAssetFolderPath = CoreAssetFolderPath.GetPath("pet", "vup");
    }

    private void InitPrivate()
    {
        
    }
}