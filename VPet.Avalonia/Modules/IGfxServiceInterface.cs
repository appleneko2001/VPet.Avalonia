using VPet.Avalonia.Systems.Graphics;
using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Modules;

/// <summary>
/// For external module use. 
/// </summary>
public interface IGfxServiceInterface
{
    void Init(string baseAssetPath);
    void CreateSpriteSheetCacheIfNotExists(string cacheFolder, uint cacheSize, Action<double>? progressCallback);
    void CreateAnimationSequencesInfo(uint cacheSize);
    IEnumerable<SpriteSheetSequenceInfo> SearchSequences(Func<PetGfxInfo, bool> condition);
    void PreloadAllGfxSequences();
}