using Avalonia;

namespace VPet.Avalonia.Systems.Graphics.Sprites;

public class SpriteSheetFrameInfo
{
    public int DelayMilliseconds { get; set; }
    
    public int Index { get; set; }
    
    public Rect Area { get; set; }
}