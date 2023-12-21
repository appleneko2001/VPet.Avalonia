using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace VPet.Avalonia.Linux.Dbus;

public class FreeDesktopNotificationUnit
{
    private const string BusName = "org.freedesktop.Notifications";
    private const string Path = "/org/freedesktop/Notifications";
    private const string Interface = "org.freedesktop.Notifications";

    private const string MethodName = "Notify";

    public async Task ExecuteAsync(Connection con, string appName, string title, string text, int timeoutMs = 10000)
    {
        await con.CallMethodAsync(Generate(con, appName, title, text, timeoutMs));
    }

    private MessageBuffer Generate(Connection con, string appName, string title, string text, int timeoutMs = 10000)
    {
        var hints = new Dictionary<string, string>();
        using var writer = con.GetMessageWriter();

        writer.WriteMethodCallHeader(
            destination: BusName, path: Path, @interface: Interface, member: MethodName,
            // ReSharper disable once StringLiteralTypo
            signature: "susssasa{sv}i");
        
        writer.WriteString(appName);
        writer.WriteUInt32(0);
        writer.WriteString("");
        writer.WriteString(title);
        writer.WriteString(text);
        writer.WriteArray(Array.Empty<string>());
        writer.WriteDictionary(hints);
        writer.WriteInt32(10000);

        return writer.CreateMessage();
    }
}