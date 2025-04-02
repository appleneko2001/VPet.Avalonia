using System.IO;
using VPet.Avalonia.Providers.VPetSimulator.Enums;
using VPet.Avalonia.Providers.VPetSimulator.Models;

namespace VPet.Avalonia.Providers.VPetSimulator;

public class VPetSimModelInstance(VPetSimPetDataModel descriptor, string rootPath)
{
    private readonly VPetSimPetDataModel _descriptor = descriptor;
    private readonly string _rootPath = Path.Combine(rootPath, descriptor[PetDataDescriptorProperty.Path]);
    
    
    
    public string? GfxAssetsPath => _rootPath;
}