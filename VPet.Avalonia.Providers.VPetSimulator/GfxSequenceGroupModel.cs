using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VPet.Avalonia.Enums;
using VPet.Avalonia.Systems.Graphics;

namespace VPet.Avalonia.Providers.VPetSimulator;

public class GfxSequenceGroupModel
{
    internal PetActivityState Activity;
    internal PetState Health;
    internal string? Tags;
    internal IReadOnlyList<PetGfxInfo> Sequences => Group.Select(a => a.Key)
        .ToImmutableArray();

    internal IReadOnlyList<KeyValuePair<PetGfxInfo, string>> Group { get; set; } = null!;
}