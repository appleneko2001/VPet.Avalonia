using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Systems.Graphics;

public class GfxSequenceGroupWeakRef : IGfxSequenceUsage
{
    private Func<SpriteSheetSequenceInfo> stateIn;
    private Func<SpriteSheetSequenceInfo> stateWithin;
    private Func<SpriteSheetSequenceInfo> stateOut;

    public GfxSequenceGroupWeakRef(Func<SpriteSheetSequenceInfo> stateIn, Func<SpriteSheetSequenceInfo> stateWithin,
        Func<SpriteSheetSequenceInfo> stateOut)
    {
        this.stateIn = stateIn;
        this.stateWithin = stateWithin;
        this.stateOut = stateOut;
    }

    public ISpriteSheet? GetStateIn() => stateIn();

    public ISpriteSheet? GetStateWithin() => stateWithin();

    public ISpriteSheet? GetStateOut() => stateOut();
}