using VPet.Avalonia.Debugging;

namespace VPet.Avalonia.Systems.Graphics;

public class GfxAssetsIndex
{
    private Dictionary<PetGfxInfo, string> Indexes = new ();

    protected void AddIndexInternal(PetGfxInfo key, string path)
    {
        if (Indexes.TryGetValue(key, out var found))
            this.WriteLine(MessageSeverity.Error, $"Unable to index GFX asset: The key is exist already ({key} -> {found})");
        
        Indexes.Add(key, path);
    }

    public IEnumerable<string> GetAllAssetPaths() => Indexes.Values;

    public IEnumerable<string> GetAllSpritePaths() => Indexes
        .Where(a => !a.Key.IsSingle)
        .Select(a => a.Value);

    public IEnumerable<KeyValuePair<PetGfxInfo, string>> GetAllAssets() => Indexes;

    public IEnumerable<string> Search(Func<PetGfxInfo, bool> cond) => 
        Indexes
            .Where(pair => cond(pair.Key))
            .Select(a => a.Value);

    public PetGfxInfo? FindKey(string asset) =>
        Indexes
            .Where(pair => pair.Value == asset)
            .Select(a => a.Key)
            .FirstOrDefault();

    public PetGfxInfo? GetInfo(string spriteAssetPath) =>
        Indexes
            .Where(pair => pair.Value == spriteAssetPath)
            .Select(a => a.Key)
            .FirstOrDefault();
}