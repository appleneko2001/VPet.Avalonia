using VPet.Avalonia.Primitives;

namespace VPet.Avalonia.Providers.VPetSimulator.Options;

public class GfxServiceInitOptions : OptionsTable
{
    public int SpriteSize
    {
        get => (int)(this["SpriteSize"] ?? 500);
        set => this["SpriteSize"] = value;
    }
}