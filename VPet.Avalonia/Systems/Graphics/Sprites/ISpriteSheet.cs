namespace VPet.Avalonia.Systems.Graphics.Sprites;

public interface ISpriteSheet
{
    uint CellSize { get; }

    int CellCounts { get; }
    
    long Duration { get; }
    
    IBitmapInstance? Sprite { get; }

    bool TryGetFrame(int index, out SpriteSheetFrameInfo? frame);
}