using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Systems.Graphics;

public interface IGfxSequenceUsage
{
    ISpriteSheet? GetStateIn();
    ISpriteSheet? GetStateWithin();
    ISpriteSheet? GetStateOut();
}