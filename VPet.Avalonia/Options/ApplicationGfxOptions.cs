using VPet.Avalonia.Primitives;

namespace VPet.Avalonia.Options;

public class ApplicationGfxOptions : OptionsTable
{
    public string CacheFolderPath
    {
        get => (string)this[nameof(CacheFolderPath)]!;
        set => this[nameof(CacheFolderPath)] = value;
    }
}