using VPet.Avalonia.Interops;
using VPet.Avalonia.Linux.Dbus;

namespace VPet.Avalonia.Linux.Interops;

/// <summary>
/// Platform-specific bridge for Linux Platform, but you cant use it directly.
/// Uses DBus to communicate system modules like notifications, power management and etc.
/// </summary>
public abstract class LinuxPlatformBridgeBase : PlatformSpecificBridge
{
    public string AppName => "VPet.Avalonia";

    private FreeDesktopNotificationUnit NotificationUnit = new();

    protected async void PostNotificationInternalAsync(string? title, string? text)
    {
        using var connection = Tmds.DBus.Protocol.Connection.Session;
        await connection.ConnectAsync();
        await NotificationUnit.ExecuteAsync(connection, AppName, title ?? "warning", text ?? string.Empty);
    }

    public override void ShowMessageBoxNative(string? title, string? text)
    {
        PostNotificationInternalAsync(title, text);
    }
}