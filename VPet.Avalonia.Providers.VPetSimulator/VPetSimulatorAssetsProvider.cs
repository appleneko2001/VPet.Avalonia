using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using VPet.Avalonia.Extensions;
using VPet.Avalonia.Systems.Contents;

namespace VPet.Avalonia.Providers.VPetSimulator;

/// <summary>
/// VPetSimulator assets provider. Please install https://store.steampowered.com/app/1920960/VPetSimulator/
/// and copy folder "mod" to folder "VPetSimulator" (case-insensitive, multiple folders with same name is not allowed.) 
/// </summary>
public class VPetSimulatorAssetsProvider : AssetsProvider
{
    //private const string AssetsFolderName = "VPetSimulator";

    /// <summary>
    /// The main working directory for the module.
    /// </summary>
    internal readonly string RootFolderPath;
    
    /// <summary>
    /// The VPetSimulator core assets folder path.
    /// </summary>
    //internal readonly string CoreAssetFolderPath;

    internal readonly string ModulesFolderPath;

    //internal readonly string LinePutScriptPath;

    /// <summary>
    /// The VPetSimulator GFX assets folder path.
    /// </summary>
    //internal readonly string GfxAssetFolderPath;

    internal IList<VPetSimModelInstance>? PetModulesList;
    
    internal VPetSimulatorAssetsProvider(string rootPath)
    {
        var assetRootPath = rootPath;//.CreateFolderThroughPath(AssetsFolderName);
        string modFolderPath;
        
        try
        {
            modFolderPath = assetRootPath.GetPath("mod");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException("Assets dependencies is not found. Please re-download the game. " +
                                                 $"Be make sure the folder \"mod\" is exist in folder \"{rootPath}\" and not empty.");
        }
        
        RootFolderPath = assetRootPath;
        //CoreAssetFolderPath = modFolderPath.GetPath("0000_core");
        ModulesFolderPath = modFolderPath;
        //GfxAssetFolderPath = CoreAssetFolderPath.GetPath("pet", "vup");
        //LinePutScriptPath = Path.Combine(CoreAssetFolderPath, "pet", "vup.lps");
    }

    public void FetchModules()
    {
        var modsPath = ModulesFolderPath;

        var mods = Directory.EnumerateDirectories(modsPath).ToImmutableArray();
        var instances = new List<VPetSimModelInstance>();

        foreach (var modPath in mods)
        {
            var modInfo = VPetSimLinePutScriptAdapter.Current
                .ReadPetInfo(Path.Combine(modPath, "info.lps"));

            var petAssetsPath = Path.Combine(modPath, "pet");

            if (Directory.Exists(petAssetsPath))
            {
                var assetsDataDesc = VPetSimLinePutScriptAdapter.Current
                    .ReadPetDataDescriptor(Path.Combine(petAssetsPath, "vup.lps"));

                var mod = new VPetSimModelInstance(assetsDataDesc!, petAssetsPath);
                instances.Add(mod);
            }
        }

        PetModulesList = instances;
    }

    public VPetSimModelInstance? SelectPreferredModel()
    {
        return PetModulesList?.FirstOrDefault() ?? throw new InvalidOperationException("Not fetched modules yet.");
    }

    private void InitPrivate()
    {
        
    }
}