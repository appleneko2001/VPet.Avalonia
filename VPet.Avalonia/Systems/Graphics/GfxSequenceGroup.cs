using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Systems.Graphics;

public class GfxSequenceGroup : IGfxSequenceUsage
{
    public IReadOnlyList<SpriteSheetSequenceInfo>? FadeInPool { get; set; }
    public IReadOnlyList<SpriteSheetSequenceInfo>? WithinPool { get; set; }
    public IReadOnlyList<SpriteSheetSequenceInfo>? FadeOutPool { get; set; }

    private Random _random = new ();

    internal void SetRandom(Random r)
    {
        _random = r;
    }
    
    public ISpriteSheet? GetStateIn()
    {
        var a = FadeInPool;
        return a[_random.Next(0, a.Count)];
    }

    public ISpriteSheet? GetStateWithin()
    {
        var a = WithinPool;
        return a[_random.Next(0, a.Count)];
    }

    public ISpriteSheet? GetStateOut()
    {
        var a = FadeOutPool;
        return a[_random.Next(0, a.Count)];
    }
}