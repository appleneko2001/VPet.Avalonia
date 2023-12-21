using VPet.Avalonia.Services.Interfaces;

namespace VPet.Avalonia.Linux.Services;

/// <summary>
/// Steam rich presence service
/// </summary>
public class SteamRichPresenceService : IRichPresenceService
{
    public string Id => "steam";
}