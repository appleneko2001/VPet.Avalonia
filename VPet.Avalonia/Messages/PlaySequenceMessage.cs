using VPet.Avalonia.Systems.Graphics.Sprites;

namespace VPet.Avalonia.Messages;

#if USE_OBSOLETE_ANIMATION_SYSTEM
//[Obsolete("SUCH WAY TO PUT ANIMATION IS SUCKS", true)]
public class PlaySequenceMessage
{
    public SpriteSheetSequenceInfo? SequenceInfo;
    public Action? OnSequenceComplete;
    public bool DoImmediately;
}

#endif