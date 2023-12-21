using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Systems.Graphics.Queues;

public class GfxSequenceQueue
{
    public ISpriteSheet? Sequence;
    public Action? OnSequenceComplete;
    public bool IsInLoop;
    public bool IsValid => _isValid;
    
    private bool _isValid = true;

    private int _index = -1;

    public bool TryGetNextSequence(ref SpriteSheetFrameInfo? frame)
    {
        if (_index >= Sequence?.CellCounts)
            return false;

        _index++;
        return Sequence?.TryGetFrame(_index, out frame) ?? throw new InvalidOperationException();
    }

    public void Invalidate() => _isValid = false;

    public override string ToString()
    {
        return Sequence?.ToString() ?? "";
    }
}