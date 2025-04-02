using System.Drawing;
using DynamicData;
using Pastel;

namespace VPet.Avalonia.Debugging;

public static class DebuggerConsole
{
    private static List<Type> IgnoreTypeList = new ();
    private static bool _enableDebug = false;

    public static void SwitchDebug(bool isEnable) => _enableDebug = isEnable;

    internal static void IgnoreObjectDebugMessage(this object source)
    {
        var type = source.GetType();
        IgnoreTypeList.ReplaceOrAdd(type, type);
    }

    internal static void UnIgnoreObjectDebugMessage(this object source)
    {
        var type = source.GetType();
        IgnoreTypeList.Remove(type);
    }

    public static void WriteLineLazy(this object source, MessageSeverity level, Func<string> getter)
        => WriteLineLazy(source.GetType(), level, getter);

    public static void WriteLineLazy(Type type, MessageSeverity level, Func<string> getter)
    {
        if(!_enableDebug)
            return; 
        
        if (IgnoreTypeList.Contains(type))
            return;
        
        PostWriteLinePrivate(type, level, getter());
    }

    public static void WriteLine(this object source, MessageSeverity level, string text) =>
        WriteLine(source.GetType(), level, text);
    
    public static void WriteLine(Type type, MessageSeverity level, string text)
    {
        if(!_enableDebug)
            return; 
        
        if (IgnoreTypeList.Contains(type))
            return;

        PostWriteLinePrivate(type, level, text);
    }

    private static void PostWriteLinePrivate(Type type, MessageSeverity level, string text)
    {
        var name = type
            .GetCustomAttributes(typeof(DebuggerObjectNameAttribute), true)
            .FirstOrDefault() is DebuggerObjectNameAttribute customNameSource ?
            customNameSource.Name : type.Name;
        var oldForeColour = Console.ForegroundColor;

        Console.ForegroundColor = level switch
        {
            MessageSeverity.Debug or MessageSeverity.Verb => ConsoleColor.DarkCyan,
            MessageSeverity.Info => ConsoleColor.White,
            MessageSeverity.Warn => ConsoleColor.Yellow,
            MessageSeverity.Error or MessageSeverity.Severe => ConsoleColor.Red,
            _ => ConsoleColor.DarkRed
        };
        
        var color = level switch
        {
            MessageSeverity.Debug => Color.DarkCyan,
            MessageSeverity.Verb => Color.LightGreen,
            MessageSeverity.Info => Color.White,
            MessageSeverity.Warn => Color.GreenYellow,
            MessageSeverity.Error => Color.OrangeRed,
            MessageSeverity.Severe => Color.Red,
            _ => Color.White
        };
        
        Console.WriteLine(FormatTextPrivate(name, level, text).Pastel(color));

        Console.ForegroundColor = oldForeColour;
    }

    private static string FormatTextPrivate(string sourceName, MessageSeverity level, string text)
    {
        return $"[{DateTime.Now}][{level.ToString()}][{sourceName}] {text}";
    }
}