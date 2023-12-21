using VPet.Avalonia.Enums;

namespace VPet.Avalonia.Systems.Graphics.Sprites;

public class SpriteSheetSequenceInfo : ISpriteSheet
{
    public string? Name { get; set; }
    
    public List<SpriteSheetFrameInfo> Frames { get; internal set; } = new();
    
    public uint CellSize { get; set; }

    public int CellCounts => Frames.Count;
    
    public long Duration => Frames.Sum(f => f.DelayMilliseconds);

    public IBitmapInstance Sprite { get; set; } = null!;

    public bool TryGetFrame(int index, out SpriteSheetFrameInfo? frame)
    {
        frame = null;
        
        if (Frames.Count == 0)
            return false;

        if (index >= Frames.Count)
            return false;

        frame = Frames[index];
        return true;
    }

    public GfxAnimationType AnimationType { get; internal set; }

    public override string ToString()
    {
        return $"({Name}, {Duration}ms)";
    }
}