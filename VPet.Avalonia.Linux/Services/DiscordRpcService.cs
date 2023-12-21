using VPet.Avalonia.Services.Interfaces;

namespace VPet.Avalonia.Linux.Services;

/// <summary>
/// Discord rich presence support.
/// </summary>
public class DiscordRpcService : IRichPresenceService
{
    public string Id => "discord_rpc";
}